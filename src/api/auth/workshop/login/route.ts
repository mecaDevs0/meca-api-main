/**
 * Endpoint de login para oficinas
 * POST /auth/workshop/login
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { AuthService } from "../../../services/auth"

export const AUTHENTICATE = false

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { email, password } = req.body

  if (!email || !password) {
    return res.status(400).json({
      message: "Email e senha são obrigatórios"
    })
  }

  try {
    // Buscar oficina pelo email
    const oficinaService = req.scope.resolve("oficinaModuleService")
    const oficinas = await oficinaService.list({
      email,
      take: 1
    })

    if (oficinas.length === 0) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    const oficina = oficinas[0]

    // Verificar se tem senha configurada
    const passwordHash = oficina.metadata?.password_hash as string

    if (!passwordHash) {
      return res.status(401).json({
        message: "Conta sem senha configurada. Use recuperação de senha."
      })
    }

    // Verificar senha
    const isPasswordValid = await AuthService.comparePassword(password, passwordHash)

    if (!isPasswordValid) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    // Gerar token JWT
    const token = AuthService.generateToken({
      id: oficina.id,
      email: oficina.email,
      type: 'workshop'
    })

    return res.json({
      message: "Login realizado com sucesso!",
      access_token: token,
      token_type: "Bearer",
      expires_in: 604800, // 7 dias
      workshop: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        status: oficina.status,
        metadata: oficina.metadata
      }
    })

  } catch (error) {
    console.error("Erro ao fazer login da oficina:", error)

    return res.status(500).json({
      message: "Erro ao fazer login",
      error: error.message
    })
  }
}
