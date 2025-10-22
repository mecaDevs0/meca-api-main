import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"
import { EmailService } from "../../../../services/email"
import crypto from 'crypto'

export const AUTHENTICATE = false

/**
 * POST /auth/customer/forgot-password
 * 
 * Solicita recuperação de senha para cliente
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const { email } = req.body

  if (!email) {
    return res.status(400).json({
      message: "Email é obrigatório"
    })
  }

  try {
    // Buscar cliente pelo email
    const customers = await customerService.listCustomers({ email })

    if (customers.length === 0) {
      // Por segurança, retornar sucesso mesmo se email não existir
      return res.json({
        message: "Se o email existir em nossa base, você receberá instruções para redefinir sua senha."
      })
    }

    const customer = customers[0]

    // Gerar token de recuperação (6 dígitos)
    const resetToken = crypto.randomInt(100000, 999999).toString()
    const resetTokenExpiry = new Date(Date.now() + 60 * 60 * 1000) // 1 hora

    // Salvar token no metadata do cliente
    await customerService.updateCustomers(customer.id, {
      metadata: {
        ...customer.metadata,
        reset_token: resetToken,
        reset_token_expiry: resetTokenExpiry.toISOString()
      }
    })

    // Enviar email
    const emailSent = await EmailService.sendPasswordReset(
      customer.email,
      customer.first_name || 'Cliente',
      resetToken,
      'customer'
    )

    if (!emailSent) {
      console.error('Erro ao enviar email de recuperação para:', customer.email)
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


