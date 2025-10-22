import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"

export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  try {
    const { id } = req.params

    // Marcar notificação como enviada
    const notification = await oficinaModuleService.updateNotificationStatus({
      id,
      status: 'sent',
      sent_at: new Date()
    })

    res.json({ 
      notification: {
        id: notification.id,
        status: notification.status,
        sent_at: notification.sent_at
      }
    })
  } catch (error) {
    res.status(500).json({ error: error.message })
  }
}
















