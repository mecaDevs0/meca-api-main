import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { BOOKING_MODULE } from "../../../../../../../modules/booking"
import { BookingStatus } from "../../../../../../../modules/booking/models/booking"
import { OFICINA_MODULE } from "../../../../../../../modules/oficina"

/**
 * POST /store/workshops/me/bookings/:id/suggest-time
 * 
 * Sugere um novo horário para um agendamento
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const eventBusService = req.scope.resolve("eventBus")
  
  const userId = req.auth_context?.actor_id
  const { id: bookingId } = req.params
  const { suggested_date, reason } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  if (!suggested_date) {
    return res.status(400).json({
      message: "suggested_date é obrigatório"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    
    // Buscar agendamento
    const booking = await bookingModuleService.retrieveBooking(bookingId)
    
    // Verificar se o agendamento pertence a esta oficina
    if (booking.oficina_id !== oficinaId) {
      return res.status(403).json({ message: "Acesso negado" })
    }
    
    // Verificar se o status permite sugestão de novo horário
    if (booking.status !== BookingStatus.PENDENTE_OFICINA) {
      return res.status(400).json({
        message: "Este agendamento não pode ter horário sugerido. Status atual: " + booking.status
      })
    }
    
    // Atualizar agendamento com sugestão de novo horário
    const updatedBooking = await bookingModuleService.updateBookings(bookingId, {
      status: BookingStatus.AGUARDANDO_CONFIRMACAO_CLIENTE,
      suggested_date: new Date(suggested_date),
      oficina_notes: reason || "Nova sugestão de horário",
      suggested_at: new Date(),
    })
    
    // Emitir evento para notificar o cliente
    await eventBusService.emit("booking.time_suggested", {
      booking_id: bookingId,
      customer_id: booking.customer_id,
      oficina_id: oficinaId,
      original_date: booking.appointment_date,
      suggested_date: new Date(suggested_date),
      reason: reason || "Nova sugestão de horário",
    })
    
    return res.json({
      message: "Novo horário sugerido com sucesso",
      booking: updatedBooking
    })
    
  } catch (error) {
    console.error("Erro ao sugerir novo horário:", error)
    
    return res.status(500).json({
      message: "Erro ao sugerir novo horário",
      error: error.message
    })
  }
}















