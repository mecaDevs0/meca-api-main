/**
 * Endpoint de login para clientes
 * POST /store/login
 */

import { Modules } from "@medusajs/framework/utils"
import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { AuthService } from "../../../services/auth"

export const AUTHENTICATE = false

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const { email, password } = req.body

  if (!email || !password) {
    return res.status(400).json({
      message: "Email e senha são obrigatórios"
    })
  }

  try {
    // Buscar cliente pelo email (otimizado)
    const customers = await customerService.listCustomers({ 
      email,
      take: 1
    })

    if (customers.length === 0) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    const customer = customers[0]

    // Verificar se tem senha configurada
    const passwordHash = customer.metadata?.password_hash as string

    if (!passwordHash) {
      return res.status(401).json({
        message: "Conta sem senha configurada. Use recuperação de senha."
      })
    }

    // Verificar senha usando bcrypt
    const isPasswordValid = await AuthService.comparePassword(password, passwordHash)

    if (!isPasswordValid) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    // Gerar token JWT
    const token = AuthService.generateToken({
      id: customer.id,
      email: customer.email,
      type: 'customer'
    })

    return res.json({
      message: "Login realizado com sucesso!",
      access_token: token,
      token_type: "Bearer",
      expires_in: 604800, // 7 dias
      customer: {
        id: customer.id,
        email: customer.email,
        first_name: customer.first_name,
        last_name: customer.last_name,
        phone: customer.phone,
        metadata: customer.metadata
      }
    })

  } catch (error) {
    console.error("Erro ao fazer login:", error)

    return res.status(500).json({
      message: "Erro ao fazer login",
      error: error.message
    })
  }
}
