import { Module } from "@medusajs/framework/utils"
import VehicleModuleService from "./service"

export const VEHICLE_MODULE = "vehicleModuleService"

export default Module(VEHICLE_MODULE, {
  service: VehicleModuleService,
})

export * from "./models/vehicle"



