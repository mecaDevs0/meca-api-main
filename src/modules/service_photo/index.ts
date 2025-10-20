import ServicePhotoModuleService from "./service"
import { Module } from "@medusajs/framework/utils"

export const SERVICE_PHOTO_MODULE = "service_photo"

export default Module(SERVICE_PHOTO_MODULE, {
  service: ServicePhotoModuleService,
})

