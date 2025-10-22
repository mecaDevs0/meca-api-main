/**
 * Endpoint para redefinir senha de clientes
 * POST /auth/customer/reset-password
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"
import { AuthService } from "../../../services/auth"

export const AUTHENTICATE = false

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const { token, password } = req.body

  if (!token || !password) {
    return res.status(400).json({
      message: "Token e nova senha são obrigatórios"
    })
  }

  if (password.length < 6) {
    return res.status(400).json({
      message: "A senha deve ter pelo menos 6 caracteres"
    })
  }

  try {
    // Buscar cliente com token válido
    const customers = await customerService.listCustomers({
      take: 1000 // Buscar todos para filtrar por metadata
    })

    const customer = customers.find(c => {
      const metadata = c.metadata as any
      return metadata?.password_reset_token === token &&
             metadata?.password_reset_expires &&
             new Date(metadata.password_reset_expires) > new Date()
    })

    if (!customer) {
      return res.status(400).json({
        message: "Token inválido ou expirado"
      })
    }

    // Hash da nova senha
    const passwordHash = await AuthService.hashPassword(password)

    // Atualizar senha e limpar token
    await customerService.updateCustomers(customer.id, {
      metadata: {
        ...customer.metadata,
        password_hash: passwordHash,
        password_reset_token: null,
        password_reset_expires: null
      }
    })

    return res.json({
      message: "Senha redefinida com sucesso! Você já pode fazer login com a nova senha"
    })

  } catch (error) {
    console.error("Erro ao redefinir senha:", error)

    return res.status(500).json({
      message: "Erro ao redefinir senha",
      error: error.message
    })
  }
}
