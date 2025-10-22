import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { VEHICLE_MODULE } from "../../../../modules/vehicle"

/**
 * GET /store/vehicles/:id
 * 
 * Busca um veículo específico do cliente autenticado
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  const customerId = req.auth_context?.actor_id
  const { id } = req.params
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const vehicle = await vehicleModuleService.retrieveVehicle(id)
  
  // Verificar se o veículo pertence ao cliente
  if (vehicle.customer_id !== customerId) {
    return res.status(403).json({ message: "Acesso negado" })
  }
  
  return res.json({ vehicle })
}

/**
 * PUT /store/vehicles/:id
 * 
 * Atualiza um veículo do cliente autenticado
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  const customerId = req.auth_context?.actor_id
  const { id } = req.params
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  // Verificar propriedade do veículo
  const existingVehicle = await vehicleModuleService.retrieveVehicle(id)
  
  if (existingVehicle.customer_id !== customerId) {
    return res.status(403).json({ message: "Acesso negado" })
  }
  
  const { marca, modelo, ano, placa, cor, km_atual, combustivel, observacoes } = req.body
  
  const vehicle = await vehicleModuleService.updateVehicles(id, {
    marca,
    modelo,
    ano,
    placa,
    cor,
    km_atual,
    combustivel,
    observacoes
  })
  
  return res.json({ vehicle })
}

/**
 * DELETE /store/vehicles/:id
 * 
 * Remove um veículo do cliente autenticado
 */
export async function DELETE(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  const customerId = req.auth_context?.actor_id
  const { id } = req.params
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  // Verificar propriedade do veículo
  const existingVehicle = await vehicleModuleService.retrieveVehicle(id)
  
  if (existingVehicle.customer_id !== customerId) {
    return res.status(403).json({ message: "Acesso negado" })
  }
  
  await vehicleModuleService.deleteVehicles(id)
  
  return res.status(204).send()
}

