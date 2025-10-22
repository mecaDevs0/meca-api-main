import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"
import { AuthService } from "../../../../services/auth"

export const AUTHENTICATE = false

/**
 * POST /auth/customer/reset-password
 * 
 * Redefine a senha do cliente usando o token recebido por email
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const { token, email, password } = req.body

  if (!token || !email || !password) {
    return res.status(400).json({
      message: "Token, email e nova senha são obrigatórios"
    })
  }

  if (password.length < 6) {
    return res.status(400).json({
      message: "A senha deve ter pelo menos 6 caracteres"
    })
  }

  try {
    // Buscar cliente pelo email
    const customers = await customerService.listCustomers({ email })

    if (customers.length === 0) {
      return res.status(401).json({
        message: "Token inválido ou expirado"
      })
    }

    const customer = customers[0]

    // Verificar token
    const storedToken = customer.metadata?.reset_token as string
    const tokenExpiry = customer.metadata?.reset_token_expiry as string

    if (!storedToken || !tokenExpiry) {
      return res.status(401).json({
        message: "Token inválido ou expirado"
      })
    }

    if (storedToken !== token) {
      return res.status(401).json({
        message: "Token inválido"
      })
    }

    if (new Date(tokenExpiry) < new Date()) {
      return res.status(401).json({
        message: "Token expirado. Solicite uma nova recuperação de senha."
      })
    }

    // Hash da nova senha
    const passwordHash = await AuthService.hashPassword(password)

    // Atualizar senha e limpar tokens
    await customerService.updateCustomers(customer.id, {
      metadata: {
        ...customer.metadata,
        password_hash: passwordHash,
        reset_token: null,
        reset_token_expiry: null
      }
    })

    return res.json({
      message: "Senha redefinida com sucesso! Você já pode fazer login com a nova senha."
    })

  } catch (error) {
    console.error("Erro ao redefinir senha:", error)

    return res.status(500).json({
      message: "Erro ao redefinir senha",
      error: error.message
    })
  }
}


