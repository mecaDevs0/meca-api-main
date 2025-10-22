/**
 * Endpoints para veículos de clientes
 * GET /api/customer/vehicles - Listar veículos do cliente
 * POST /api/customer/vehicles - Adicionar veículo
 * PUT /api/customer/vehicles/[id] - Atualizar veículo
 * DELETE /api/customer/vehicles/[id] - Remover veículo
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

// Middleware de autenticação JWT
async function authenticateCustomer(req: MedusaRequest): Promise<any> {
  const authHeader = req.headers.authorization
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    throw new Error('Token de autenticação necessário')
  }

  const token = authHeader.substring(7)
  const { AuthService } = await import("../../../services/auth")
  
  try {
    const decoded = AuthService.verifyToken(token) as any
    if (decoded.type !== 'customer') {
      throw new Error('Token inválido para cliente')
    }
    return decoded
  } catch (error) {
    throw new Error('Token inválido')
  }
}

/**
 * GET /api/customer/vehicles
 * Listar veículos do cliente
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const customer = await authenticateCustomer(req)
    const vehicleService = req.scope.resolve("vehicleModuleService")

    const vehicles = await vehicleService.list({
      customer_id: customer.id,
      take: 50,
      order: { created_at: "DESC" }
    })

    return res.json({
      vehicles: vehicles.map(vehicle => ({
        id: vehicle.id,
        marca: vehicle.marca,
        modelo: vehicle.modelo,
        ano: vehicle.ano,
        placa: vehicle.placa,
        cor: vehicle.cor,
        km_atual: vehicle.km_atual,
        combustivel: vehicle.combustivel,
        observacoes: vehicle.observacoes,
        created_at: vehicle.created_at
      })),
      count: vehicles.length
    })

  } catch (error) {
    console.error("Erro ao listar veículos:", error)
    return res.status(401).json({
      message: "Não autorizado",
      error: error.message
    })
  }
}

/**
 * POST /api/customer/vehicles
 * Adicionar veículo
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const customer = await authenticateCustomer(req)
    const { marca, modelo, ano, placa, cor, km_atual, combustivel, observacoes } = req.body

    if (!marca || !modelo || !ano || !placa) {
      return res.status(400).json({
        message: "Marca, modelo, ano e placa são obrigatórios"
      })
    }

    const vehicleService = req.scope.resolve("vehicleModuleService")

    // Verificar se placa já existe
    const existingVehicles = await vehicleService.list({
      placa,
      take: 1
    })

    if (existingVehicles.length > 0) {
      return res.status(409).json({
        message: "Veículo com esta placa já está cadastrado"
      })
    }

    // Criar veículo
    const vehicle = await vehicleService.create({
      customer_id: customer.id,
      marca,
      modelo,
      ano: parseInt(ano),
      placa,
      cor,
      km_atual: km_atual ? parseInt(km_atual) : null,
      combustivel,
      observacoes
    })

    return res.status(201).json({
      message: "Veículo cadastrado com sucesso!",
      vehicle: {
        id: vehicle.id,
        marca: vehicle.marca,
        modelo: vehicle.modelo,
        ano: vehicle.ano,
        placa: vehicle.placa,
        cor: vehicle.cor,
        km_atual: vehicle.km_atual,
        combustivel: vehicle.combustivel,
        observacoes: vehicle.observacoes
      }
    })

  } catch (error) {
    console.error("Erro ao cadastrar veículo:", error)
    
    if (error.message.includes('Token')) {
      return res.status(401).json({
        message: "Não autorizado",
        error: error.message
      })
    }

    return res.status(500).json({
      message: "Erro ao cadastrar veículo",
      error: error.message
    })
  }
}
