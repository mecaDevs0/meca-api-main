import { MedusaService } from "@medusajs/framework/utils"
import Booking from "./models/booking"

/**
 * BookingModuleService
 * 
 * Serviço principal do módulo Booking.
 * Gerencia os agendamentos de serviços do marketplace.
 */

class BookingModuleService extends MedusaService({ Booking }) {}

export default BookingModuleService

