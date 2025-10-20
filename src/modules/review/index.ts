import { Module } from "@medusajs/framework/utils"
import Review from "./models/review"
import ReviewModuleService from "./service"

export const REVIEW_MODULE = "review"

const reviewModule = Module(REVIEW_MODULE, {
  service: ReviewModuleService,
})

export default reviewModule

// Exportar linkable para uso em Module Links
export const linkable = {
  review: {
    serviceName: REVIEW_MODULE,
    primaryKey: "id",
    model: Review,
  },
}

export * from "./models/review"

