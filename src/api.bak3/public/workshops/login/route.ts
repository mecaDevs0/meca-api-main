import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../modules/oficina"
import { AuthService } from "../../../../services/auth"

export const AUTHENTICATE = false

/**
 * POST /public/workshops/login
 * 
 * Login de oficina com autenticação REAL usando bcrypt e JWT
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaService = req.scope.resolve(OFICINA_MODULE)

  const { email, password } = req.body

  if (!email || !password) {
    return res.status(400).json({
      message: "Email e senha são obrigatórios"
    })
  }

  try {
    // Buscar oficina pelo email
    const oficinas = await oficinaService.listOficinas({ email })

    if (oficinas.length === 0) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    const oficina = oficinas[0]

    // Verificar se a oficina foi aprovada
    if (oficina.status !== 'aprovado') {
      return res.status(403).json({
        message: `Sua oficina ainda não foi aprovada. Status atual: ${oficina.status}`,
        status: oficina.status
      })
    }

    // Verificar senha REAL usando bcrypt
    const passwordHash = oficina.metadata?.password_hash as string

    if (!passwordHash) {
      return res.status(500).json({
        message: "Erro de configuração. Entre em contato com o suporte."
      })
    }

    const isPasswordValid = await AuthService.comparePassword(password, passwordHash)

    if (!isPasswordValid) {
      return res.status(401).json({
        message: "Email ou senha inválidos"
      })
    }

    // Gerar token JWT REAL
    const token = AuthService.generateToken({
      id: oficina.id,
      email: oficina.email,
      type: 'workshop'
    })

    return res.json({
      message: "Login realizado com sucesso!",
      token: token,
      oficina: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        cnpj: oficina.cnpj,
        status: oficina.status,
        phone: oficina.phone,
        address: oficina.address,
        description: oficina.description,
        horario_funcionamento: oficina.horario_funcionamento,
        logo_url: oficina.logo_url,
        photo_urls: oficina.photo_urls,
        dados_bancarios: oficina.dados_bancarios
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
