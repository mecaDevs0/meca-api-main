/**
 * Endpoints para agendamentos de oficinas
 * GET /api/workshop/bookings - Listar agendamentos da oficina
 * POST /api/workshop/bookings/[id]/confirm - Confirmar agendamento
 * POST /api/workshop/bookings/[id]/reject - Rejeitar agendamento
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

// Middleware de autenticação JWT para oficinas
async function authenticateWorkshop(req: MedusaRequest): Promise<any> {
  const authHeader = req.headers.authorization
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    throw new Error('Token de autenticação necessário')
  }

  const token = authHeader.substring(7)
  const { AuthService } = await import("../../../services/auth")
  
  try {
    const decoded = AuthService.verifyToken(token) as any
    if (decoded.type !== 'workshop') {
      throw new Error('Token inválido para oficina')
    }
    return decoded
  } catch (error) {
    throw new Error('Token inválido')
  }
}

/**
 * GET /api/workshop/bookings
 * Listar agendamentos da oficina
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const workshop = await authenticateWorkshop(req)
    const bookingService = req.scope.resolve("bookingModuleService")

    const bookings = await bookingService.list({
      oficina_id: workshop.id,
      take: 50,
      order: { created_at: "DESC" }
    })

    return res.json({
      bookings: bookings.map(booking => ({
        id: booking.id,
        appointment_date: booking.appointment_date,
        status: booking.status,
        vehicle_snapshot: booking.vehicle_snapshot,
        customer_notes: booking.customer_notes,
        oficina_notes: booking.oficina_notes,
        estimated_price: booking.estimated_price,
        final_price: booking.final_price,
        confirmed_at: booking.confirmed_at,
        completed_at: booking.completed_at,
        created_at: booking.created_at
      })),
      count: bookings.length
    })

  } catch (error) {
    console.error("Erro ao listar agendamentos da oficina:", error)
    return res.status(401).json({
      message: "Não autorizado",
      error: error.message
    })
  }
}
