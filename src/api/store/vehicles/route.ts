import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { VEHICLE_MODULE } from "../../../modules/vehicle"

/**
 * GET /store/vehicles
 * 
 * Lista todos os veículos do cliente autenticado
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  
  // Pegar customer_id do cliente autenticado
  const customerId = req.auth_context?.actor_id
  
  if (!customerId) {
    return res.status(401).json({
      message: "Não autenticado"
    })
  }
  
  // Buscar veículos do cliente
  const vehicles = await vehicleModuleService.listVehicles({
    customer_id: customerId
  })
  
  return res.json({ vehicles })
}

/**
 * POST /store/vehicles
 * 
 * Cadastra um novo veículo para o cliente autenticado
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  
  const customerId = req.auth_context?.actor_id
  
  if (!customerId) {
    return res.status(401).json({
      message: "Não autenticado"
    })
  }
  
  const { marca, modelo, ano, placa, cor, km_atual, combustivel, observacoes } = req.body
  
  // Validação básica
  if (!marca || !modelo || !ano || !placa) {
    return res.status(400).json({
      message: "Marca, modelo, ano e placa são obrigatórios"
    })
  }
  
  // Criar veículo
  const vehicle = await vehicleModuleService.createVehicles({
    customer_id: customerId,
    marca,
    modelo,
    ano,
    placa,
    cor,
    km_atual,
    combustivel,
    observacoes
  })
  
  return res.status(201).json({ vehicle })
}

