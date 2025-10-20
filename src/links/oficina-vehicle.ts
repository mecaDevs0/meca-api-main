import { defineLink } from "@medusajs/framework/utils"
import OficinaModule from "../modules/oficina"
import VehicleModule from "../modules/vehicle"

/**
 * Link: Oficina -> Vehicle
 * 
 * Link direto entre oficina e veículo para histórico de serviços
 */

export default defineLink(
  OficinaModule.linkable.oficina,
  VehicleModule.linkable.vehicle
)

