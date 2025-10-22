import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../../../modules/booking"
import { BookingStatus } from "../../../../../modules/booking/models/booking"

/**
 * POST /store/bookings/:id/confirm-payment
 * 
 * Cliente confirma o serviço finalizado e inicia o processo de pagamento
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const orderModuleService = req.scope.resolve(Modules.ORDER)
  const paymentModuleService = req.scope.resolve(Modules.PAYMENT)
  const eventBusService = req.scope.resolve("eventBus")
  
  const customerId = req.auth_context?.actor_id
  const { id: bookingId } = req.params
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    // Buscar agendamento
    const booking = await bookingModuleService.retrieveBooking(bookingId)
    
    // Verificar se o agendamento pertence ao cliente
    if (booking.customer_id !== customerId) {
      return res.status(403).json({ message: "Acesso negado" })
    }
    
    // Verificar se o status permite pagamento
    if (booking.status !== BookingStatus.FINALIZADO_MECANICO) {
      return res.status(400).json({
        message: "Este agendamento ainda não foi finalizado pela oficina. Status: " + booking.status
      })
    }
    
    // Buscar ou criar Order para este booking
    let order
    
    if (booking.order_id) {
      order = await orderModuleService.retrieveOrder(booking.order_id)
    } else {
      // Criar Order se não existir
      order = await orderModuleService.createOrders({
        currency_code: "brl",
        email: req.auth_context?.actor_email || "",
        items: [{
          variant_id: booking.product_id, // Seria o variant do product
          quantity: 1,
          unit_price: booking.final_price || booking.estimated_price,
        }],
        metadata: {
          booking_id: bookingId,
          is_booking_payment: true,
        }
      })
      
      // Atualizar booking com order_id
      await bookingModuleService.updateBookings(bookingId, {
        order_id: order.id,
      })
    }
    
    // Criar Payment Session com PagBank
    const paymentSession = await paymentModuleService.createPaymentSession({
      amount: booking.final_price || booking.estimated_price,
      currency_code: "brl",
      provider_id: "pagbank", // ID do nosso provedor customizado
      data: {
        booking_id: bookingId,
        customer_id: customerId,
      }
    })
    
    // Emitir evento
    await eventBusService.emit("booking.payment_initiated", {
      booking_id: bookingId,
      customer_id: customerId,
      order_id: order.id,
      amount: booking.final_price || booking.estimated_price,
    })
    
    return res.json({
      message: "Sessão de pagamento criada",
      booking,
      order,
      payment_session: paymentSession,
    })
    
  } catch (error) {
    console.error("Erro ao confirmar pagamento:", error)
    
    return res.status(500).json({
      message: "Erro ao processar pagamento",
      error: error.message
    })
  }
}

