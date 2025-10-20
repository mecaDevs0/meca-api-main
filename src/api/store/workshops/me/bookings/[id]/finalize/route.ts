import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { BOOKING_MODULE } from "../../../../../../../modules/booking"
import { OFICINA_MODULE } from "../../../../../../../modules/oficina"
import { BookingStatus } from "../../../../../../../modules/booking/models/booking"

/**
 * POST /store/workshops/me/bookings/:id/finalize
 * 
 * Marca um agendamento como finalizado pelo mecânico
 * Notifica o cliente para confirmar e realizar o pagamento
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
  const { final_price, oficina_notes } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
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
    
    // Verificar se o status permite finalização
    if (booking.status !== BookingStatus.CONFIRMADO) {
      return res.status(400).json({
        message: "Apenas agendamentos confirmados podem ser finalizados. Status atual: " + booking.status
      })
    }
    
    // Atualizar status para finalizado pelo mecânico
    const updatedBooking = await bookingModuleService.updateBookings(bookingId, {
      status: BookingStatus.FINALIZADO_MECANICO,
      final_price: final_price || booking.estimated_price,
      oficina_notes,
      completed_at: new Date(),
    })
    
    // Emitir evento para notificar o cliente para confirmar e pagar
    await eventBusService.emit("booking.finalized_by_workshop", {
      booking_id: bookingId,
      customer_id: booking.customer_id,
      oficina_id: oficinaId,
      final_price: final_price || booking.estimated_price,
    })
    
    return res.json({
      message: "Serviço finalizado com sucesso. Aguardando confirmação e pagamento do cliente.",
      booking: updatedBooking
    })
    
  } catch (error) {
    console.error("Erro ao finalizar agendamento:", error)
    
    return res.status(500).json({
      message: "Erro ao finalizar agendamento",
      error: error.message
    })
  }
}

