import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  try {
    // Buscar notificações pendentes para envio
    const pendingNotifications = await oficinaModuleService.listPendingNotifications({
      workshop_id: req.user.id,
      scheduled_for: new Date(),
    })

    res.json({ 
      notifications: pendingNotifications.map(notification => ({
        id: notification.id,
        type: notification.type,
        booking: {
          id: notification.booking.id,
          customer_name: notification.booking.customer_name,
          customer_phone: notification.booking.customer_phone,
          workshop_name: notification.booking.workshop.name,
          workshop_address: notification.booking.workshop.address,
          service_name: notification.booking.service.title,
          service_price: notification.booking.service.price,
          estimated_duration: notification.booking.service.duration_minutes,
          appointment_date: notification.booking.appointment_date,
        }
      }))
    })
  } catch (error) {
    res.status(500).json({ error: error.message })
  }
}




