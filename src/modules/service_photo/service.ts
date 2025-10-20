import { MedusaService } from "@medusajs/framework/utils"
import ServicePhoto from "./models/service-photo"

class ServicePhotoModuleService extends MedusaService({
  ServicePhoto,
}) {}

export default ServicePhotoModuleService

