import { defineLink } from "@medusajs/framework/utils"
import OrderModule from "@medusajs/medusa/order"
import BookingModule from "../modules/booking"

/**
 * Link: Booking -> Order
 * 
 * Vincula um agendamento ao pedido (order) criado após a finalização do serviço.
 * O fluxo é: Booking (agendamento) -> Service (serviço realizado) -> Order (pedido/pagamento)
 */

export default defineLink(
  BookingModule.linkable.booking,
  OrderModule.linkable.order
)



