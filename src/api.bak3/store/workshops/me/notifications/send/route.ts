import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  try {
    const { phone_number, email, message, type } = req.body

    // Enviar notificação via SMS e Email
    const result = await oficinaModuleService.sendNotification({
      workshop_id: req.user.id,
      phone_number,
      email,
      message,
      type
    })

    res.json({ 
      success: result.success,
      sms_result: result.sms_result,
      email_result: result.email_result,
      error: result.error
    })
  } catch (error) {
    res.status(500).json({ error: error.message })
  }
}
















