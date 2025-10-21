import { Module } from "@medusajs/framework/utils"
import VehicleModuleService from "./services/vehicle"

export const VEHICLE_MODULE = "vehicleModuleService"

export default Module(VEHICLE_MODULE, {
  service: VehicleModuleService,
})
/**
 * Definição do Módulo Vehicle
 * 
 * Este módulo gerencia os veículos dos clientes
 */

export const VEHICLE_MODULE = "vehicle"

const vehicleModule = Module(VEHICLE_MODULE, {
  service: VehicleModuleService,
})
export default vehicleModule

// Exportar linkable para uso em Module Links
export const linkable = {
  vehicle: {
    serviceName: VEHICLE_MODULE,
    primaryKey: "id",
    model: Vehicle,
  },
}

export * from "./models/vehicle"


export * from "./models/vehicle"



