import { SubscriberArgs, SubscriberConfig } from "@medusajs/framework"
import { FCM_MODULE } from "../modules/fcm_notification"

/**
 * Subscriber: Booking Notifications
 * 
 * Envia notificações push quando eventos de agendamento ocorrem
 */

export default async function bookingNotificationsHandler({
  event,
  container,
}: SubscriberArgs<any>) {
  const { data, name: event_name } = event
  const fcmService = container.resolve(FCM_MODULE)
  const logger = container.resolve("logger")
  
  try {
    switch (event_name) {
      case "booking.created":
        await fcmService.send({
          to: data.oficina_id,
          title: "Novo Agendamento! 🔔",
          body: "Você recebeu um novo pedido de agendamento. Confira os detalhes!",
          data: {
            type: "new_booking",
            booking_id: data.booking_id,
          }
        })
        logger.info(`Notificação de novo agendamento enviada para oficina ${data.oficina_id}`)
        break
        
      case "booking.confirmed":
        await fcmService.send({
          to: data.customer_id,
          title: "Agendamento Confirmado! ✅",
          body: "Sua oficina confirmou o agendamento. Prepare-se para o atendimento!",
          data: {
            type: "booking_confirmed",
            booking_id: data.booking_id,
          }
        })
        logger.info(`Notificação de confirmação enviada para cliente ${data.customer_id}`)
        break
        
      case "booking.rejected":
        await fcmService.send({
          to: data.customer_id,
          title: "Agendamento Recusado ❌",
          body: `Infelizmente sua solicitação foi recusada. Motivo: ${data.reason}`,
          data: {
            type: "booking_rejected",
            booking_id: data.booking_id,
            reason: data.reason,
          }
        })
        logger.info(`Notificação de rejeição enviada para cliente ${data.customer_id}`)
        break
        
      case "booking.finalized_by_workshop":
        await fcmService.send({
          to: data.customer_id,
          title: "Serviço Finalizado! 🎉",
          body: `Seu serviço foi concluído! Confirme e realize o pagamento de R$ ${(data.final_price / 100).toFixed(2)}`,
          data: {
            type: "booking_finalized",
            booking_id: data.booking_id,
            final_price: data.final_price,
          }
        })
        logger.info(`Notificação de serviço finalizado enviada para cliente ${data.customer_id}`)
        break
        
      default:
        logger.warn(`Evento não tratado: ${event_name}`)
    }
    
  } catch (error) {
    logger.error(`Erro ao enviar notificação para evento ${event_name}:`, error)
  }
}

export const config: SubscriberConfig = {
  event: ["booking.created", "booking.confirmed", "booking.rejected", "booking.finalized_by_workshop"],
}

