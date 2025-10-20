import { SubscriberArgs, SubscriberConfig } from "@medusajs/framework"
import { SYSTEM_ALERT_MODULE } from "../modules/system_alert"
import { BOOKING_MODULE } from "../modules/booking"
import { REVIEW_MODULE } from "../modules/review"

/**
 * Anomaly Detector - Detector de Anomalias Automático
 * 
 * Monitora eventos e gera alertas quando detecta comportamentos suspeitos
 */

export default async function anomalyDetectorHandler({
  event: { data, event_name },
  container,
}: SubscriberArgs<any>) {
  const alertService = container.resolve(SYSTEM_ALERT_MODULE)
  const bookingService = container.resolve(BOOKING_MODULE)
  const reviewService = container.resolve(REVIEW_MODULE)
  const logger = container.resolve("logger")
  
  try {
    switch (event_name) {
      case "booking.rejected":
        // Detectar oficina rejeitando muitos agendamentos
        await checkHighRejectionRate(data.oficina_id, bookingService, alertService)
        break
        
      case "review.created":
        // Detectar avaliações muito baixas
        if (data.rating <= 2) {
          await alertService.createSystemAlerts({
            type: "warning",
            severity: "high",
            category: "review",
            title: "Avaliação Baixa Detectada",
            message: `Oficina ${data.oficina_id} recebeu avaliação ${data.rating}/5`,
            entity_type: "workshop",
            entity_id: data.oficina_id,
            action_required: true,
            details: {
              review_id: data.review_id,
              rating: data.rating,
              comment: data.comment
            },
            triggered_by: "low_rating_detector"
          })
        }
        break
        
      case "booking.created":
        // Detectar spike de agendamentos (possível fraude/bot)
        await checkBookingSpike(data.customer_id, bookingService, alertService)
        break
        
      case "order.payment_failed":
        // Alerta de falha de pagamento
        await alertService.createSystemAlerts({
          severity: "high",
          category: "payment",
          title: "Falha de Pagamento",
          message: `Pagamento falhou para o pedido ${data.order_id}`,
          entity_type: "order",
          entity_id: data.order_id,
          action_required: true,
          details: data,
          triggered_by: "payment_failure_detector"
        })
        break
    }
    
  } catch (error) {
    logger.error(`Erro no detector de anomalias: ${error.message}`)
  }
}

async function checkHighRejectionRate(
  oficinaId: string,
  bookingService: any,
  alertService: any
) {
  // Buscar últimos 10 agendamentos da oficina
  const recentBookings = await bookingService.listBookings(
    { oficina_id: oficinaId },
    { take: 10, order: { created_at: "DESC" } }
  )
  
  if (recentBookings.length < 5) return // Amostra pequena
  
  const rejectedCount = recentBookings.filter(b => b.status === 'recusado').length
  const rejectionRate = (rejectedCount / recentBookings.length) * 100
  
  if (rejectionRate > 50) {
    await alertService.createSystemAlerts({
      type: "warning",
      severity: "medium",
      category: "workshop",
      title: "Alta Taxa de Rejeição",
      message: `Oficina ${oficinaId} está rejeitando ${rejectionRate.toFixed(0)}% dos agendamentos`,
      entity_type: "workshop",
      entity_id: oficinaId,
      action_required: true,
      details: {
        rejection_rate: rejectionRate,
        total_bookings: recentBookings.length,
        rejected_count: rejectedCount
      },
      triggered_by: "high_rejection_detector"
    })
  }
}

async function checkBookingSpike(
  customerId: string,
  bookingService: any,
  alertService: any
) {
  // Buscar agendamentos do cliente nas últimas 24h
  const dayAgo = new Date()
  dayAgo.setHours(dayAgo.getHours() - 24)
  
  const recentBookings = await bookingService.listBookings({
    customer_id: customerId,
    created_at: { $gte: dayAgo.toISOString() }
  })
  
  // Mais de 5 agendamentos em 24h é suspeito
  if (recentBookings.length > 5) {
    await alertService.createSystemAlerts({
      type: "security",
      severity: "high",
      category: "booking",
      title: "Possível Atividade Suspeita",
      message: `Cliente ${customerId} criou ${recentBookings.length} agendamentos em 24h`,
      entity_type: "client",
      entity_id: customerId,
      action_required: true,
      details: {
        booking_count: recentBookings.length,
        period_hours: 24
      },
      triggered_by: "booking_spike_detector"
    })
  }
}

export const config: SubscriberConfig = {
  event: [
    "booking.rejected",
    "review.created",
    "booking.created",
    "order.payment_failed"
  ],
}

