import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import crypto from "crypto"
import { EntityManager } from "typeorm"
import { Booking } from "../../models/booking"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  
  try {
    console.log('🔔 Webhook PagBank recebido:', JSON.stringify(req.body, null, 2))
    
    // Verificar assinatura do webhook (se configurada)
    const signature = req.headers['x-pagbank-signature'] as string
    const webhookSecret = process.env.PAGBANK_WEBHOOK_SECRET
    
    if (webhookSecret && signature) {
      const expectedSignature = crypto
        .createHmac('sha256', webhookSecret)
        .update(JSON.stringify(req.body))
        .digest('hex')
      
      if (signature !== expectedSignature) {
        console.error('❌ Assinatura do webhook inválida')
        res.status(401).json({ error: 'Assinatura inválida' })
        return
      }
    }

    const webhookData = req.body
    const { event, data } = webhookData

    // Processar apenas eventos de pagamento
    if (event !== 'payment.approved' && event !== 'payment.denied') {
      console.log(`⚠️ Evento ignorado: ${event}`)
      res.status(200).json({ message: 'Evento ignorado' })
      return
    }

    const paymentId = data?.id
    const bookingId = data?.metadata?.booking_id

    if (!bookingId) {
      console.error('❌ booking_id não encontrado no webhook')
      res.status(400).json({ error: 'booking_id não encontrado' })
      return
    }

    // Buscar agendamento
    const booking = await manager.findOne(Booking, {
      where: { id: bookingId },
      relations: ['customer', 'workshop', 'service']
    })

    if (!booking) {
      console.error(`❌ Agendamento não encontrado: ${bookingId}`)
      res.status(404).json({ error: 'Agendamento não encontrado' })
      return
    }

    if (event === 'payment.approved') {
      await handlePaymentApproved(manager, booking, paymentId, data)
    } else if (event === 'payment.denied') {
      await handlePaymentDenied(manager, booking, paymentId, data)
    }

    // Responder imediatamente ao PagBank
    res.status(200).json({ message: 'Webhook processado com sucesso' })

  } catch (error) {
    console.error('❌ Erro no webhook PagBank:', error)
    res.status(500).json({ error: 'Erro interno do servidor' })
  }
}

async function handlePaymentApproved(
  manager: EntityManager,
  booking: Booking,
  paymentId: string,
  paymentData: any
) {
  try {
    console.log(`✅ Pagamento aprovado para agendamento: ${booking.id}`)
    
    // Atualizar status do agendamento
    await manager.update(Booking, 
      { id: booking.id },
      { 
        status: 'paid',
        payment_id: paymentId,
        payment_status: 'approved',
        paid_at: new Date(),
        payment_data: paymentData
      }
    )

    // Disparar histórico de manutenção
    await triggerMaintenanceHistory(manager, booking)

    // Enviar notificações
    await sendPaymentNotifications(booking, 'approved')

    console.log(`✅ Agendamento ${booking.id} marcado como pago`)

  } catch (error) {
    console.error(`❌ Erro ao processar pagamento aprovado:`, error)
    throw error
  }
}

async function handlePaymentDenied(
  manager: EntityManager,
  booking: Booking,
  paymentId: string,
  paymentData: any
) {
  try {
    console.log(`❌ Pagamento negado para agendamento: ${booking.id}`)
    
    // Atualizar status do agendamento
    await manager.update(Booking, 
      { id: booking.id },
      { 
        status: 'payment_failed',
        payment_id: paymentId,
        payment_status: 'denied',
        payment_data: paymentData
      }
    )

    // Enviar notificações
    await sendPaymentNotifications(booking, 'denied')

    console.log(`❌ Agendamento ${booking.id} marcado como pagamento falhado`)

  } catch (error) {
    console.error(`❌ Erro ao processar pagamento negado:`, error)
    throw error
  }
}

async function triggerMaintenanceHistory(manager: EntityManager, booking: Booking) {
  try {
    // Criar registro no histórico de manutenção
    const maintenanceRecord = {
      id: `maint_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      vehicle_id: booking.vehicle_id,
      customer_id: booking.customer_id,
      workshop_id: booking.workshop_id,
      workshop_name: booking.workshop.name,
      service_id: booking.service_id,
      service_name: booking.service.name,
      service_description: booking.service.description,
      completion_date: booking.completed_at || new Date(),
      price_paid: booking.total_amount,
      notes: booking.notes,
      workshop_notes: booking.workshop_notes,
      created_at: new Date(),
      updated_at: new Date()
    }

    await manager.query(`
      INSERT INTO maintenance_history (
        id, vehicle_id, customer_id, workshop_id, workshop_name,
        service_id, service_name, service_description, completion_date,
        price_paid, notes, workshop_notes, created_at, updated_at
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14)
    `, [
      maintenanceRecord.id,
      maintenanceRecord.vehicle_id,
      maintenanceRecord.customer_id,
      maintenanceRecord.workshop_id,
      maintenanceRecord.workshop_name,
      maintenanceRecord.service_id,
      maintenanceRecord.service_name,
      maintenanceRecord.service_description,
      maintenanceRecord.completion_date,
      maintenanceRecord.price_paid,
      maintenanceRecord.notes,
      maintenanceRecord.workshop_notes,
      maintenanceRecord.created_at,
      maintenanceRecord.updated_at
    ])

    console.log(`✅ Histórico de manutenção criado: ${maintenanceRecord.id}`)

  } catch (error) {
    console.error(`❌ Erro ao criar histórico de manutenção:`, error)
    // Não falhar o webhook por causa do histórico
  }
}

async function sendPaymentNotifications(booking: Booking, status: 'approved' | 'denied') {
  try {
    const notificationService = require('../../services/notification-service')
    
    if (status === 'approved') {
      // Notificação para o cliente
      await notificationService.send({
        type: 'payment_approved',
        customerId: booking.customer_id,
        data: {
          bookingId: booking.id,
          serviceName: booking.service.name,
          workshopName: booking.workshop.name,
          amount: booking.total_amount
        }
      })

      // Notificação para a oficina
      await notificationService.send({
        type: 'payment_received',
        workshopId: booking.workshop_id,
        data: {
          bookingId: booking.id,
          customerName: booking.customer.name,
          serviceName: booking.service.name,
          amount: booking.total_amount
        }
      })
    } else {
      // Notificação de pagamento negado
      await notificationService.send({
        type: 'payment_denied',
        customerId: booking.customer_id,
        data: {
          bookingId: booking.id,
          serviceName: booking.service.name,
          workshopName: booking.workshop.name
        }
      })
    }

    console.log(`✅ Notificações de pagamento enviadas para agendamento ${booking.id}`)

  } catch (error) {
    console.error(`❌ Erro ao enviar notificações:`, error)
    // Não falhar o webhook por causa das notificações
  }
}



















