import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { EntityManager } from "typeorm"
import { Booking } from "../../../models/booking"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const bookingId = req.params.id
  const { newDate, suggestedBy } = req.body

  if (!newDate || !suggestedBy) {
    res.status(400).json({ error: "newDate e suggestedBy são obrigatórios" })
    return
  }

  if (!['cliente', 'oficina'].includes(suggestedBy)) {
    res.status(400).json({ error: "suggestedBy deve ser 'cliente' ou 'oficina'" })
    return
  }

  try {
    // Buscar agendamento
    const booking = await manager.findOne(Booking, {
      where: { id: bookingId },
      relations: ['customer', 'workshop']
    })

    if (!booking) {
      res.status(404).json({ error: "Agendamento não encontrado" })
      return
    }

    // Validar se o agendamento pode ser alterado
    if (booking.status !== 'pending_oficina' && booking.status !== 'pending_cliente') {
      res.status(400).json({ 
        error: "Agendamento não pode ser alterado no status atual" 
      })
      return
    }

    // Validar nova data (não pode ser no passado)
    const suggestedDateTime = new Date(newDate)
    const now = new Date()
    
    if (suggestedDateTime <= now) {
      res.status(400).json({ 
        error: "Nova data deve ser no futuro" 
      })
      return
    }

    // Determinar novo status baseado em quem sugeriu
    let newStatus: string
    if (suggestedBy === 'oficina') {
      newStatus = 'pending_cliente'
    } else {
      newStatus = 'pending_oficina'
    }

    // Atualizar agendamento
    await manager.update(Booking, 
      { id: bookingId },
      {
        suggested_date: suggestedDateTime,
        suggested_by: suggestedBy,
        status: newStatus,
        updated_at: new Date()
      }
    )

    // Enviar notificação
    await sendScheduleNotification(booking, suggestedBy, suggestedDateTime)

    res.json({
      success: true,
      data: {
        bookingId,
        newStatus,
        suggestedDate: suggestedDateTime,
        suggestedBy
      }
    })

  } catch (error) {
    console.error("Erro ao sugerir nova data:", error)
    res.status(500).json({ error: "Erro interno do servidor" })
  }
}

async function sendScheduleNotification(
  booking: Booking, 
  suggestedBy: string, 
  suggestedDate: Date
) {
  try {
    const notificationService = require('../../../services/notification-service')
    
    if (suggestedBy === 'oficina') {
      // Notificar cliente sobre sugestão da oficina
      await notificationService.send({
        type: 'schedule_suggested_by_workshop',
        customerId: booking.customer_id,
        data: {
          bookingId: booking.id,
          workshopName: booking.workshop.name,
          suggestedDate: suggestedDate.toISOString(),
          serviceName: booking.service?.name || 'Serviço'
        }
      })
    } else {
      // Notificar oficina sobre sugestão do cliente
      await notificationService.send({
        type: 'schedule_suggested_by_customer',
        workshopId: booking.workshop_id,
        data: {
          bookingId: booking.id,
          customerName: booking.customer.name,
          suggestedDate: suggestedDate.toISOString(),
          serviceName: booking.service?.name || 'Serviço'
        }
      })
    }

    console.log(`✅ Notificação de sugestão de horário enviada para ${suggestedBy}`)

  } catch (error) {
    console.error(`❌ Erro ao enviar notificação de sugestão:`, error)
  }
}













