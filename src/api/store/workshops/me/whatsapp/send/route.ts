import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  try {
    const { phone_number, message } = req.body

    // Enviar mensagem via WhatsApp
    const result = await oficinaModuleService.sendWhatsAppMessage({
      workshop_id: req.user.id,
      phone_number,
      message
    })

    res.json({ 
      success: result.success,
      message_id: result.message_id,
      error: result.error
    })
  } catch (error) {
    res.status(500).json({ error: error.message })
  }
}




