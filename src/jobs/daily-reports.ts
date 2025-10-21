import { MedusaContainer } from "@medusajs/framework"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../modules/booking"
import { OFICINA_MODULE } from "../modules/oficina"

/**
 * Scheduled Job: Daily Reports
 * 
 * Gera relatórios diários e envia para administradores
 * Executa diariamente às 23:00
 */

export default async function dailyReportsJob(container: MedusaContainer) {
  const logger = container.resolve("logger")
  const eventBus = container.resolve("eventBus")
  
  try {
    logger.info("Starting daily reports generation")
    
    const customerService = container.resolve(Modules.CUSTOMER)
    const bookingService = container.resolve(BOOKING_MODULE)
    const oficinaService = container.resolve(OFICINA_MODULE)
    
    // Estatísticas do dia
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    
    const tomorrow = new Date(today)
    tomorrow.setDate(tomorrow.getDate() + 1)
    
    // Contadores
    const [
      newCustomers,
      newBookings,
      confirmedBookings,
      newWorkshops,
      pendingWorkshops
    ] = await Promise.all([
      customerService.listCustomers({
        created_at: { $gte: today, $lt: tomorrow }
      }),
      bookingService.listBookings({
        created_at: { $gte: today, $lt: tomorrow }
      }),
      bookingService.listBookings({
        status: "confirmado",
        confirmed_at: { $gte: today, $lt: tomorrow }
      }),
      oficinaService.listOficinas({
        created_at: { $gte: today, $lt: tomorrow }
      }),
      oficinaService.listOficinas({
        status: "pendente"
      })
    ])
    
    const report = {
      date: today.toISOString().split('T')[0],
      metrics: {
        new_customers: newCustomers.length,
        new_bookings: newBookings.length,
        confirmed_bookings: confirmedBookings.length,
        new_workshops: newWorkshops.length,
        pending_workshops: pendingWorkshops.length
      },
      generated_at: new Date().toISOString()
    }
    
    // Emitir evento para processar relatório (enviar email, salvar, etc)
    await eventBus.emit("report.daily.generated", report)
    
    logger.info("Daily reports generated:", report.metrics)
    
    return report
  } catch (error) {
    logger.error("Error generating daily reports:", error)
    throw error
  }
}

export const config = {
  name: "daily-reports",
  schedule: "0 23 * * *", // Diariamente às 23:00
  data: {},
}

