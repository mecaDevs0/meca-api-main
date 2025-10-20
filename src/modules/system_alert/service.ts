import { MedusaService } from "@medusajs/framework/utils"
import SystemAlert from "./models/system-alert"

class SystemAlertModuleService extends MedusaService({
  SystemAlert,
}) {}

export default SystemAlertModuleService

