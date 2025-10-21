import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { OFICINA_MODULE } from "@medusajs/modules-sdk"
import { AuthService } from "../../../../services/auth"

export const AUTHENTICATE = false

/**
 * POST /auth/workshop/reset-password
 * 
 * Redefine a senha da oficina usando o token recebido por email
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaService = req.scope.resolve(OFICINA_MODULE)
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
    // Buscar oficina pelo email
    const oficinas = await oficinaService.listOficinas({ email })

    if (oficinas.length === 0) {
      return res.status(401).json({
        message: "Token inválido ou expirado"
      })
    }

    const oficina = oficinas[0]

    // Verificar token
    const storedToken = oficina.metadata?.reset_token as string
    const tokenExpiry = oficina.metadata?.reset_token_expiry as string

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
    await oficinaService.updateOficinas(oficina.id, {
      metadata: {
        ...oficina.metadata,
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

