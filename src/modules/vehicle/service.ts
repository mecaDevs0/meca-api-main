import { MedusaService } from "@medusajs/framework/utils"
import Vehicle from "./models/vehicle"

/**
 * VehicleModuleService
 * 
 * Serviço principal do módulo Vehicle.
 * Gerencia os veículos dos clientes.
 */

class VehicleModuleService extends MedusaService({ Vehicle }) {}

export default VehicleModuleService

