import { MedusaService } from "@medusajs/framework/utils"
import Review from "./models/review"

/**
 * ReviewModuleService
 * 
 * Gerencia avaliações de oficinas e serviços
 */

class ReviewModuleService extends MedusaService({ Review }) {}

export default ReviewModuleService

