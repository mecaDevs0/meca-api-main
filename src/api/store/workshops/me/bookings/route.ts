import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { BOOKING_MODULE } from "../../../../../modules/booking"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * GET /store/workshops/me/bookings
 * 
 * Lista todos os agendamentos recebidos pela oficina autenticada
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }
    
    const oficinaId = oficinas[0].id
    
    // Buscar agendamentos da oficina
    const { status, limit = 50, offset = 0 } = req.query
    
    const filters: any = {
      oficina_id: oficinaId
    }
    
    if (status) {
      filters.status = status
    }
    
    const bookings = await bookingModuleService.listBookings(filters, {
      take: Number(limit),
      skip: Number(offset),
      order: { appointment_date: "ASC" }
    })
    
    return res.json({
      bookings,
      count: bookings.length,
    })
    
  } catch (error) {
    console.error("Erro ao listar agendamentos:", error)
    
    return res.status(500).json({
      message: "Erro ao listar agendamentos",
      error: error.message
    })
  }
}

