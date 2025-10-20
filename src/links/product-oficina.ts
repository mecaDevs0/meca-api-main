import { defineLink } from "@medusajs/framework/utils"
import ProductModule from "@medusajs/medusa/product"
import OficinaModule from "../modules/oficina"

/**
 * Link: Product -> Oficina
 * 
 * Vincula serviços (Products) às oficinas que os oferecem.
 * Uma oficina pode oferecer múltiplos serviços.
 * Um serviço pode ser oferecido por múltiplas oficinas.
 */

export default defineLink(
  ProductModule.linkable.product,
  OficinaModule.linkable.oficina
)



