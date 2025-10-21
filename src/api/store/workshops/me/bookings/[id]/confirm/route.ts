import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../../../../../modules/booking"
import { OFICINA_MODULE } from "../../../../../../../modules/oficina"
import { BookingStatus } from "../../../../../../../modules/booking/models/booking"
import { EmailService } from "../../../../../../../services/email"

/**
 * POST /store/workshops/me/bookings/:id/confirm
 * 
 * Confirma um agendamento recebido pela oficina
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const eventBusService = req.scope.resolve("eventBus")
  
  const userId = req.auth_context?.actor_id
  const { id: bookingId } = req.params
  
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
    
    // Verificar se o status permite confirmação
    if (booking.status !== BookingStatus.PENDENTE_OFICINA) {
      return res.status(400).json({
        message: "Este agendamento não pode ser confirmado. Status atual: " + booking.status
      })
    }
    
    // Atualizar status para confirmado
    const updatedBooking = await bookingModuleService.updateBookings(bookingId, {
      status: BookingStatus.CONFIRMADO,
      confirmed_at: new Date(),
    })
    
    // Buscar dados do cliente para enviar email
    const customer = await customerService.retrieveCustomer(booking.customer_id)
    const oficina = oficinas[0]
    
    // Enviar email de confirmação ao cliente (não bloquear resposta)
    if (customer && customer.email) {
      EmailService.sendBookingConfirmed(
        customer.email,
        customer.first_name || 'Cliente',
        oficina.name,
        booking.service_title || 'Serviço',
        new Date(booking.scheduled_at)
      ).catch(err => {
        console.error('Erro ao enviar email de confirmação:', err)
      })
    }
    
    // Emitir evento para notificar o cliente
    await eventBusService.emit("booking.confirmed", {
      booking_id: bookingId,
      customer_id: booking.customer_id,
      oficina_id: oficinaId,
    })
    
    return res.json({
      message: "Agendamento confirmado com sucesso",
      booking: updatedBooking
    })
    
  } catch (error) {
    console.error("Erro ao confirmar agendamento:", error)
    
    return res.status(500).json({
      message: "Erro ao confirmar agendamento",
      error: error.message
    })
  }
}

