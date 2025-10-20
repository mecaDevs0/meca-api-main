import { defineLink } from "@medusajs/framework/utils"
import OrderModule from "@medusajs/medusa/order"
import ReviewModule from "../modules/review"

/**
 * Link: Review -> Order
 * 
 * Vincula uma avaliação ao pedido (order) correspondente.
 * Garante que o cliente só pode avaliar serviços que de fato realizou.
 */

export default defineLink(
  ReviewModule.linkable.review,
  OrderModule.linkable.order
)



