import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../modules/oficina"
import { AuthService } from "../../../services/auth"

export const AUTHENTICATE = false

/**
 * POST /store/workshops
 *
 * Cadastro de uma nova oficina com senha REAL criptografada
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const userModuleService = req.scope.resolve(Modules.USER)

  const {
    // Dados do usuário/dono
    email,
    password,
    first_name,
    last_name,

    // Dados da oficina
    name,
    cnpj,
    phone,
    address,
    description,
    logo_url,
    photo_urls,
    horario_funcionamento
  } = req.body

  // Validações básicas
  if (!email || !password || !name || !cnpj) {
    return res.status(400).json({
      message: "Email, senha, nome da oficina e CNPJ são obrigatórios"
    })
  }

  if (password.length < 6) {
    return res.status(400).json({
      message: "A senha deve ter no mínimo 6 caracteres"
    })
  }

  try {
    // Verificar se já existe oficina com esse email ou CNPJ
    const existingOficinas = await oficinaModuleService.listOficinas({
      $or: [
        { email },
        { cnpj }
      ]
    })

    if (existingOficinas.length > 0) {
      return res.status(409).json({
        message: "Já existe uma oficina cadastrada com este email ou CNPJ"
      })
    }

    // Criptografar a senha REAL
    const hashedPassword = await AuthService.hashPassword(password)

    // 1. Criar usuário para o dono da oficina
    let user
    try {
      user = await userModuleService.createUsers({
        email,
        first_name: first_name || name,
        last_name: last_name || "",
      })
    } catch (error) {
      // Se já existir, buscar
      const existingUsers = await userModuleService.listUsers({ email })
      user = existingUsers[0]
    }

    // 2. Criar a oficina com senha CRIPTOGRAFADA armazenada no metadata
    const oficina = await oficinaModuleService.createOficinas({
      name,
      cnpj,
      email,
      phone,
      address,
      description,
      logo_url,
      photo_urls,
      horario_funcionamento,
      status: "pendente",
      metadata: {
        user_id: user.id,
        password_hash: hashedPassword // Armazenar senha CRIPTOGRAFADA
      }
    })

    return res.status(201).json({
      message: "Oficina cadastrada com sucesso! Aguardando aprovação do administrador.",
      oficina: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        cnpj: oficina.cnpj,
        status: oficina.status
      }
    })

  } catch (error) {
    console.error("Erro ao cadastrar oficina:", error)

    return res.status(500).json({
      message: "Erro ao cadastrar oficina",
      error: error.message
    })
  }
}
