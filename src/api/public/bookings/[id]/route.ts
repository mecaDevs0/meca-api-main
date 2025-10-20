import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { BOOKING_MODULE } from "../../../../modules/booking"

export const AUTHENTICATE = false

/**
 * PUT /public/bookings/:id
 * 
 * Atualizar status do agendamento (aceitar/rejeitar)
 */
export async function PUT(
  req: MedusaRequest<{ id: string }>,
  res: MedusaResponse
) {
  const bookingService = req.scope.resolve(BOOKING_MODULE)
  const id = req.params.id
  const { status, rejection_reason } = req.body

  if (!status) {
    return res.status(400).json({
      message: "Status é obrigatório"
    })
  }

  if (!['pendente', 'confirmado', 'cancelado', 'concluido'].includes(status)) {
    return res.status(400).json({
      message: "Status inválido. Use: pendente, confirmado, cancelado ou concluido"
    })
  }

  try {
    const booking = await bookingService.retrieveBooking(id)

    if (!booking) {
      return res.status(404).json({
        message: "Agendamento não encontrado"
      })
    }

    const updatedBooking = await bookingService.updateBookings(id, {
      status,
      metadata: {
        ...booking.metadata,
        rejection_reason: rejection_reason || undefined
      }
    })

    return res.json({
      message: "Agendamento atualizado com sucesso!",
      booking: updatedBooking
    })

  } catch (error) {
    console.error("Erro ao atualizar agendamento:", error)

    return res.status(500).json({
      message: "Erro ao atualizar agendamento",
      error: error.message
    })
  }
}

/**
 * GET /public/bookings/:id
 * 
 * Buscar agendamento por ID
 */
export async function GET(
  req: MedusaRequest<{ id: string }>,
  res: MedusaResponse
) {
  const bookingService = req.scope.resolve(BOOKING_MODULE)
  const id = req.params.id

  try {
    const booking = await bookingService.retrieveBooking(id)

    if (!booking) {
      return res.status(404).json({
        message: "Agendamento não encontrado"
      })
    }

    return res.json({
      booking
    })

  } catch (error) {
    console.error("Erro ao buscar agendamento:", error)

    return res.status(500).json({
      message: "Erro ao buscar agendamento",
      error: error.message
    })
  }
}

