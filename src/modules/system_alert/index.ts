import SystemAlertModuleService from "./service"
import { Module } from "@medusajs/framework/utils"

export const SYSTEM_ALERT_MODULE = "system_alert"

export default Module(SYSTEM_ALERT_MODULE, {
  service: SystemAlertModuleService,
})

