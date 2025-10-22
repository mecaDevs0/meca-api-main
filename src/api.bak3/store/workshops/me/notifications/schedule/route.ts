import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  try {
    const { booking_id, type, scheduled_for } = req.body

    // Criar notificação agendada
    const notification = await oficinaModuleService.createScheduledNotification({
      workshop_id: req.user.id,
      booking_id,
      type,
      scheduled_for: new Date(scheduled_for),
      status: 'pending'
    })

    res.json({ 
      notification: {
        id: notification.id,
        type: notification.type,
        scheduled_for: notification.scheduled_for,
        status: notification.status
      }
    })
  } catch (error) {
    res.status(500).json({ error: error.message })
  }
}
















