/**
 * Endpoint de recuperação de senha para clientes
 * POST /auth/customer/forgot-password
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"
import EmailService from "../../../services/email"
import { randomBytes } from "crypto"

export const AUTHENTICATE = false

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
    const customers = await customerService.listCustomers({ 
      email,
      take: 1
    })

    if (customers.length === 0) {
      // Por segurança, retornar sucesso mesmo se email não existir
      return res.json({
        message: "Se o email existir, você receberá instruções para redefinir sua senha"
      })
    }

    const customer = customers[0]

    // Gerar token de reset
    const resetToken = randomBytes(32).toString('hex')
    const resetExpires = new Date(Date.now() + 3600000) // 1 hora

    // Salvar token no metadata do cliente
    await customerService.updateCustomers(customer.id, {
      metadata: {
        ...customer.metadata,
        password_reset_token: resetToken,
        password_reset_expires: resetExpires.toISOString()
      }
    })

    // Enviar email de recuperação
    try {
      await EmailService.sendPasswordReset(email, resetToken, 'customer')
    } catch (emailError) {
      console.error("Erro ao enviar email de recuperação:", emailError)
      return res.status(500).json({
        message: "Erro ao enviar email de recuperação"
      })
    }

    return res.json({
      message: "Instruções para redefinir sua senha foram enviadas para seu email"
    })

  } catch (error) {
    console.error("Erro ao processar recuperação de senha:", error)

    return res.status(500).json({
      message: "Erro ao processar solicitação",
      error: error.message
    })
  }
}
