import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { FCM_NOTIFICATION_MODULE } from "../../../../modules/fcm_notification"

export const AUTHENTICATE = false

/**
 * POST /admin/notifications/send
 * 
 * Envia notificação push para usuários
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const fcmService = req.scope.resolve(FCM_NOTIFICATION_MODULE)
  
  const { title, message, target } = req.body
  
  if (!title || !message || !target) {
    return res.status(400).json({
      message: "title, message e target são obrigatórios"
    })
  }
  
  try {
    // Criar notificação
    const notification = await fcmService.createFcmNotifications({
      title,
      body: message,
      data: { target },
      sent_at: new Date(),
    })
    
    return res.status(201).json({
      message: "Notificação enviada com sucesso",
      notification,
      sent_to: target
    })
    
  } catch (error) {
    console.error("Erro ao enviar notificação:", error)
    
    return res.status(500).json({
      message: "Erro ao enviar notificação",
      error: error.message
    })
  }
}
