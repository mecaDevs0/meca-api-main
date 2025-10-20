import VehicleHistoryModuleService from "./service"
import { Module } from "@medusajs/framework/utils"

export const VEHICLE_HISTORY_MODULE = "vehicle_history"

export default Module(VEHICLE_HISTORY_MODULE, {
  service: VehicleHistoryModuleService,
})

