import VehicleModuleService from "./service"
import { Module } from "@medusajs/framework/utils"
import Vehicle from "./models/vehicle"

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

