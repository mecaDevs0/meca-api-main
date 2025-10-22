import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../modules/oficina"
import { AuthService } from "../../../services/auth"
import { EmailService } from "../../../services/email"

export const AUTHENTICATE = false

/**
 * POST /public/workshops
 *
 * Cadastro de uma nova oficina (sem autenticação)
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)

  const {
    name,
    cnpj,
    email,
    phone,
    address,
    description,
    logo_url,
    photo_urls,
    horario_funcionamento,
    password
  } = req.body

  if (!email || !name || !cnpj || !password) {
    return res.status(400).json({
      message: "Email, nome da oficina, CNPJ e senha são obrigatórios"
    })
  }

  try {
    // Verificar se já existe oficina com esse CNPJ ou email
    const existingOficinas = await oficinaModuleService.listOficinas({
      $or: [
        { cnpj },
        { email }
      ]
    })

    if (existingOficinas.length > 0) {
      return res.status(400).json({
        message: "Já existe uma oficina cadastrada com esse CNPJ ou email"
      })
    }

    // Criar hash da senha
    const passwordHash = await AuthService.hashPassword(password)

    // Criar oficina
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
        password_hash: passwordHash
      }
    })

    // Enviar email de boas-vindas (não bloquear resposta)
    EmailService.sendWelcomeWorkshop(email, name).catch(err => {
      console.error('Erro ao enviar email de boas-vindas:', err)
    })

    return res.status(201).json({
      message: "Oficina cadastrada com sucesso! Aguardando aprovação.",
      oficina: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
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

/**
 * GET /public/workshops
 *
 * Lista oficinas aprovadas (sem autenticação)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)

  const { limit = 50, offset = 0, q, cidade, estado } = req.query

  const filters: any = {
    status: "aprovado" // Apenas oficinas aprovadas
  }

  if (q) {
    filters.$or = [
      { name: { $ilike: `%${q}%` } }
    ]
  }

  if (cidade) {
    filters['address.cidade'] = { $ilike: `%${cidade}%` }
  }

  if (estado) {
    filters['address.estado'] = { $ilike: `%${estado}%` }
  }

  try {
    const workshops = await oficinaModuleService.listOficinas(filters, {
      take: Number(limit),
      skip: Number(offset),
      order: { created_at: "DESC" }
    })

    return res.json({
      workshops,
      count: workshops.length,
      limit: Number(limit),
      offset: Number(offset),
    })

  } catch (error) {
    console.error("Erro ao listar oficinas:", error)

    return res.status(500).json({
      message: "Erro ao listar oficinas",
      error: error.message
    })
  }
}

