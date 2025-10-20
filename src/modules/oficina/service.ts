import { MedusaService } from "@medusajs/framework/utils"
import Oficina from "./models/oficina"

/**
 * OficinaModuleService
 * 
 * Serviço principal do módulo Oficina.
 * Fornece métodos CRUD automáticos através do MedusaService.
 */

class OficinaModuleService extends MedusaService({ Oficina }) {}

export default OficinaModuleService

