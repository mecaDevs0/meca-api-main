import { MedusaService } from "@medusajs/framework/utils"
import MasterService from "./models/master-service"

/**
 * MasterServiceModuleService
 * 
 * Gerencia o catálogo de serviços base
 */

class MasterServiceModuleService extends MedusaService({ MasterService }) {}

export default MasterServiceModuleService

