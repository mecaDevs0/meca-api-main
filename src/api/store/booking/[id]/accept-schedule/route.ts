import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { EntityManager } from "typeorm"
import { Booking } from "../../../models/booking"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const bookingId = req.params.id

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

    // Validar se há sugestão pendente
    if (!booking.suggested_date) {
      res.status(400).json({ 
        error: "Não há sugestão de horário pendente" 
      })
      return
    }

    // Atualizar agendamento com a data sugerida
    await manager.update(Booking, 
      { id: bookingId },
      {
        scheduled_date: booking.suggested_date,
        status: 'confirmed',
        suggested_date: null,
        suggested_by: null,
        confirmed_at: new Date(),
        updated_at: new Date()
      }
    )

    // Enviar notificação de confirmação
    await sendConfirmationNotification(booking)

    res.json({
      success: true,
      data: {
        bookingId,
        confirmedDate: booking.suggested_date,
        status: 'confirmed'
      }
    })

  } catch (error) {
    console.error("Erro ao aceitar sugestão:", error)
    res.status(500).json({ error: "Erro interno do servidor" })
  }
}

async function sendConfirmationNotification(booking: Booking) {
  try {
    const notificationService = require('../../../services/notification-service')
    
    // Notificar cliente
    await notificationService.send({
      type: 'schedule_confirmed',
      customerId: booking.customer_id,
      data: {
        bookingId: booking.id,
        workshopName: booking.workshop.name,
        confirmedDate: booking.suggested_date?.toISOString(),
        serviceName: booking.service?.name || 'Serviço'
      }
    })

    // Notificar oficina
    await notificationService.send({
      type: 'schedule_confirmed',
      workshopId: booking.workshop_id,
      data: {
        bookingId: booking.id,
        customerName: booking.customer.name,
        confirmedDate: booking.suggested_date?.toISOString(),
        serviceName: booking.service?.name || 'Serviço'
      }
    })

    console.log(`✅ Notificação de confirmação enviada para agendamento ${booking.id}`)

  } catch (error) {
    console.error(`❌ Erro ao enviar notificação de confirmação:`, error)
  }
}








