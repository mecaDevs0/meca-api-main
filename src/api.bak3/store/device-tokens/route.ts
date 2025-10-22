import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { FCM_MODULE } from "../../../modules/fcm_notification"

/**
 * POST /store/device-tokens
 * 
 * Registra ou atualiza um token de dispositivo FCM para o usuário autenticado
 * 
 * Body:
 * - fcm_token: Token do FCM
 * - platform: "android" ou "ios"
 * - device_name: Nome do dispositivo (opcional)
 * - app_version: Versão do app (opcional)
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const fcmService = req.scope.resolve(FCM_MODULE)
  
  const userId = req.auth_context?.actor_id
  const actorType = req.auth_context?.actor_type // "customer" ou "user"
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const {
    fcm_token,
    platform,
    device_name,
    app_version
  } = req.body
  
  if (!fcm_token || !platform) {
    return res.status(400).json({
      message: "fcm_token e platform são obrigatórios"
    })
  }
  
  try {
    const isCustomer = actorType === "customer"
    
    const deviceToken = await fcmService.registerDeviceToken(
      userId,
      isCustomer,
      fcm_token,
      platform,
      device_name
    )
    
    return res.status(201).json({
      message: "Token de dispositivo registrado com sucesso",
      device_token: deviceToken
    })
    
  } catch (error) {
    console.error("Erro ao registrar token:", error)
    
    return res.status(500).json({
      message: "Erro ao registrar token de dispositivo",
      error: error.message
    })
  }
}

