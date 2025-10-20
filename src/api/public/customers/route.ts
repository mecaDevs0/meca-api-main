import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * POST /public/customers
 * 
 * Cadastro de novo cliente
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)

  const {
    email,
    password,
    first_name,
    last_name,
    phone
  } = req.body

  if (!email || !password || !first_name) {
    return res.status(400).json({
      message: "Email, senha e nome são obrigatórios"
    })
  }

  try {
    // Verificar se cliente já existe
    const existingCustomers = await customerService.listCustomers({ email })

    if (existingCustomers.length > 0) {
      return res.status(400).json({
        message: "Já existe um cliente cadastrado com esse email"
      })
    }

    // Criar cliente
    const customer = await customerService.createCustomers({
      email,
      first_name,
      last_name: last_name || '',
      phone,
      metadata: {
        password // Em produção, usar hash
      }
    })

    return res.status(201).json({
      message: "Cliente cadastrado com sucesso!",
      customer: {
        id: customer.id,
        email: customer.email,
        first_name: customer.first_name,
        last_name: customer.last_name
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

/**
 * POST /public/customers/login
 * 
 * Login de cliente
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const { email, password } = req.query

  if (!email || !password) {
    return res.status(400).json({
      message: "Email e senha são obrigatórios"
    })
  }

  try {
    const customers = await customerService.listCustomers({ email: email as string })

    if (customers.length === 0) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    const customer = customers[0]

    // Verificar senha (em produção, usar bcrypt)
    if (customer.metadata?.password !== password) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    return res.json({
      message: "Login realizado com sucesso!",
      customer: {
        id: customer.id,
        email: customer.email,
        first_name: customer.first_name,
        last_name: customer.last_name,
        phone: customer.phone
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

