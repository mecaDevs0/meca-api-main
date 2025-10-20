import { MedusaService } from "@medusajs/framework/utils"
import VehicleHistory from "./models/vehicle-history"

class VehicleHistoryModuleService extends MedusaService({
  VehicleHistory,
}) {}

export default VehicleHistoryModuleService

