import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../modules/oficina"
import { AuthService } from "../../../services/auth"

export const AUTHENTICATE = false

// Função para calcular distância entre duas coordenadas (fórmula de Haversine)
function calculateDistance(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const R = 6371 // Raio da Terra em km
  const dLat = (lat2 - lat1) * Math.PI / 180
  const dLon = (lon2 - lon1) * Math.PI / 180
  const a = 
    Math.sin(dLat/2) * Math.sin(dLat/2) +
    Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * 
    Math.sin(dLon/2) * Math.sin(dLon/2)
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a))
  return R * c
}

/**
 * GET /store/workshops
 *
 * Listar oficinas aprovadas com filtros de localização
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const { lat, lng, radius } = req.query

  try {
    // Buscar oficinas aprovadas
    const oficinas = await oficinaModuleService.listOficinas({
      status: "aprovado"
    })

    // Calcular distância se coordenadas fornecidas
    let workshopsWithDistance = oficinas
    if (lat && lng) {
      const userLat = parseFloat(lat as string)
      const userLng = parseFloat(lng as string)
      
      workshopsWithDistance = oficinas.map(workshop => {
        const distance = calculateDistance(
          userLat, userLng,
          workshop.latitude || 0,
          workshop.longitude || 0
        )
        return {
          ...workshop,
          distance
        }
      }).filter(workshop => {
        if (radius) {
          return workshop.distance <= parseFloat(radius as string)
        }
        return true
      }).sort((a, b) => a.distance - b.distance)
    }

    // Formatar resposta
    const formattedWorkshops = workshopsWithDistance.map(workshop => ({
      id: workshop.id,
      name: workshop.name,
      email: workshop.email,
      phone: workshop.phone,
      address: workshop.address,
      description: workshop.description,
      logo_url: workshop.logo_url,
      photo_urls: workshop.photo_urls,
      rating: workshop.rating || 4.5,
      reviews_count: workshop.reviews_count || 0,
      average_price: workshop.average_price || 0,
      distance: workshop.distance,
      latitude: workshop.latitude,
      longitude: workshop.longitude,
      horario_funcionamento: workshop.horario_funcionamento,
      status: workshop.status,
      created_at: workshop.created_at,
      updated_at: workshop.updated_at
    }))

    return res.json({
      workshops: formattedWorkshops,
      total: formattedWorkshops.length
    })

  } catch (error) {
    console.error("Erro ao buscar oficinas:", error)
    return res.status(500).json({
      message: "Erro ao buscar oficinas",
      error: error.message
    })
  }
}

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

