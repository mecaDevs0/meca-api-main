import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { VEHICLE_MODULE } from "../../../modules/vehicle"

export const AUTHENTICATE = false

/**
 * POST /public/vehicles
 * 
 * Cadastro de novo veículo
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleService = req.scope.resolve(VEHICLE_MODULE)

  const {
    customer_id,
    placa,
    marca,
    modelo,
    ano,
    cor,
    chassis
  } = req.body

  if (!customer_id || !placa || !marca || !modelo) {
    return res.status(400).json({
      message: "Cliente, placa, marca e modelo são obrigatórios"
    })
  }

  try {
    // Verificar se já existe veículo com essa placa
    const existingVehicles = await vehicleService.listVehicles({ placa })

    if (existingVehicles.length > 0) {
      return res.status(400).json({
        message: "Já existe um veículo cadastrado com essa placa"
      })
    }

    // Criar veículo
    const vehicle = await vehicleService.createVehicles({
      customer_id,
      placa,
      marca,
      modelo,
      ano: ano ? parseInt(ano) : undefined,
      cor,
      chassis
    })

    return res.status(201).json({
      message: "Veículo cadastrado com sucesso!",
      vehicle
    })

  } catch (error) {
    console.error("Erro ao cadastrar veículo:", error)

    return res.status(500).json({
      message: "Erro ao cadastrar veículo",
      error: error.message
    })
  }
}

/**
 * GET /public/vehicles?customer_id=xxx
 * 
 * Lista veículos do cliente
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleService = req.scope.resolve(VEHICLE_MODULE)
  const { customer_id } = req.query

  if (!customer_id) {
    return res.status(400).json({
      message: "ID do cliente é obrigatório"
    })
  }

  try {
    const vehicles = await vehicleService.listVehicles({ customer_id: customer_id as string })

    return res.json({
      vehicles,
      count: vehicles.length
    })

  } catch (error) {
    console.error("Erro ao listar veículos:", error)

    return res.status(500).json({
      message: "Erro ao listar veículos",
      error: error.message
    })
  }
}

