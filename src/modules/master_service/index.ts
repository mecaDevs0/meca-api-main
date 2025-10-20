import MasterServiceModuleService from "./service"
import { Module } from "@medusajs/framework/utils"

export const MASTER_SERVICE_MODULE = "master_service"

export default Module(MASTER_SERVICE_MODULE, {
  service: MasterServiceModuleService,
})

export * from "./models/master-service"

