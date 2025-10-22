/**
 * Endpoint de registro para clientes
 * POST /store/register
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"
import { AuthService } from "../../../services/auth"
import EmailService from "../../../services/email"

export const AUTHENTICATE = false

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const { email, password, first_name, last_name, phone } = req.body

  if (!email || !password || !first_name || !last_name) {
    return res.status(400).json({
      message: "Email, senha, nome e sobrenome são obrigatórios"
    })
  }

  try {
    // Verificar se cliente já existe
    const existingCustomers = await customerService.listCustomers({ 
      email,
      take: 1
    })

    if (existingCustomers.length > 0) {
      return res.status(409).json({
        message: "Cliente já cadastrado com este email"
      })
    }

    // Hash da senha
    const passwordHash = await AuthService.hashPassword(password)

    // Criar cliente
    const customer = await customerService.createCustomers({
      email,
      first_name,
      last_name,
      phone,
      metadata: {
        password_hash: passwordHash,
        created_via: 'api'
      }
    })

    // Enviar email de boas-vindas
    try {
      await EmailService.sendWelcomeCustomer({
        name: `${first_name} ${last_name}`,
        email,
        phone
      })
    } catch (emailError) {
      console.error("Erro ao enviar email de boas-vindas:", emailError)
      // Não falhar o registro por erro de email
    }

    // Gerar token JWT
    const token = AuthService.generateToken({
      id: customer.id,
      email: customer.email,
      type: 'customer'
    })

    return res.status(201).json({
      message: "Cliente cadastrado com sucesso!",
      access_token: token,
      token_type: "Bearer",
      expires_in: 604800, // 7 dias
      customer: {
        id: customer.id,
        email: customer.email,
        first_name: customer.first_name,
        last_name: customer.last_name,
        phone: customer.phone
      }
    })

  } catch (error) {
    console.error("Erro ao cadastrar cliente:", error)

    return res.status(500).json({
      message: "Erro ao cadastrar cliente",
      error: error.message
    })
  }
}
