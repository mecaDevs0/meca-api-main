import { defineLink } from "@medusajs/framework/utils"
import CustomerModule from "@medusajs/medusa/customer"
import VehicleModule from "../modules/vehicle"

/**
 * Link: Customer -> Vehicle
 * 
 * Um cliente pode ter múltiplos veículos cadastrados.
 * Este link substitui a FK direta customer_id no modelo Vehicle.
 */

export default defineLink(
  CustomerModule.linkable.customer,
  VehicleModule.linkable.vehicle
)



