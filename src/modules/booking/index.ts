import BookingModuleService from "./service"
import { Module } from "@medusajs/framework/utils"
import Booking from "./models/booking"

/**
 * Definição do Módulo Booking
 * 
 * Este módulo gerencia os agendamentos de serviços no marketplace MECA
 */

export const BOOKING_MODULE = "booking"

const bookingModule = Module(BOOKING_MODULE, {
  service: BookingModuleService,
})

export default bookingModule

// Exportar linkable para uso em Module Links
export const linkable = {
  booking: {
    serviceName: BOOKING_MODULE,
    primaryKey: "id",
    model: Booking,
  },
}

export * from "./models/booking"

