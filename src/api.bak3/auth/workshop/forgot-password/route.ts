import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"
import { EmailService } from "../../../../services/email"
import crypto from 'crypto'

export const AUTHENTICATE = false

/**
 * POST /auth/workshop/forgot-password
 * 
 * Solicita recuperação de senha para oficina
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaService = req.scope.resolve(OFICINA_MODULE)
  const { email } = req.body

  if (!email) {
    return res.status(400).json({
      message: "Email é obrigatório"
    })
  }

  try {
    // Buscar oficina pelo email
    const oficinas = await oficinaService.listOficinas({ email })

    if (oficinas.length === 0) {
      // Por segurança, retornar sucesso mesmo se email não existir
      return res.json({
        message: "Se o email existir em nossa base, você receberá instruções para redefinir sua senha."
      })
    }

    const oficina = oficinas[0]

    // Gerar token de recuperação (6 dígitos)
    const resetToken = crypto.randomInt(100000, 999999).toString()
    const resetTokenExpiry = new Date(Date.now() + 60 * 60 * 1000) // 1 hora

    // Salvar token no metadata da oficina
    await oficinaService.updateOficinas(oficina.id, {
      metadata: {
        ...oficina.metadata,
        reset_token: resetToken,
        reset_token_expiry: resetTokenExpiry.toISOString()
      }
    })

    // Enviar email
    const emailSent = await EmailService.sendPasswordReset(
      oficina.email,
      oficina.name || 'Oficina',
      resetToken,
      'workshop'
    )

    if (!emailSent) {
      console.error('Erro ao enviar email de recuperação para:', oficina.email)
    }

    return res.json({
      message: "Se o email existir em nossa base, você receberá instruções para redefinir sua senha."
    })

  } catch (error) {
    console.error("Erro ao processar recuperação de senha:", error)

    return res.status(500).json({
      message: "Erro ao processar solicitação",
      error: error.message
    })
  }
}


