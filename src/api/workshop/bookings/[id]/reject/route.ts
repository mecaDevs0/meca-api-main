/**
 * Endpoint para rejeitar agendamento
 * POST /api/workshop/bookings/[id]/reject
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import EmailService from "../../../../../services/email"

export const AUTHENTICATE = false

// Middleware de autenticação JWT para oficinas
async function authenticateWorkshop(req: MedusaRequest): Promise<any> {
  const authHeader = req.headers.authorization
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    throw new Error('Token de autenticação necessário')
  }

  const token = authHeader.substring(7)
  const { AuthService } = await import("../../../../../services/auth")
  
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

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const workshop = await authenticateWorkshop(req)
    const { id } = req.params
    const { reason } = req.body

    if (!id) {
      return res.status(400).json({
        message: "ID do agendamento é obrigatório"
      })
    }

    const bookingService = req.scope.resolve("bookingModuleService")
    
    // Buscar agendamento
    const bookings = await bookingService.list({
      id,
      oficina_id: workshop.id,
      take: 1
    })

    if (bookings.length === 0) {
      return res.status(404).json({
        message: "Agendamento não encontrado"
      })
    }

    const booking = bookings[0]

    if (booking.status !== 'pendente_oficina') {
      return res.status(400).json({
        message: "Agendamento já foi processado"
      })
    }

    // Rejeitar agendamento
    await bookingService.update(id, {
      status: 'recusado',
      oficina_notes: reason || 'Agendamento rejeitado pela oficina',
      cancelled_at: new Date().toISOString()
    })

    // Buscar dados do cliente para email
    const customerService = req.scope.resolve("customerModuleService")
    const customers = await customerService.listCustomers({
      id: booking.customer_id,
      take: 1
    })

    if (customers.length > 0) {
      const customer = customers[0]
      
      // Enviar email de rejeição
      try {
        await EmailService.sendBookingRejected({
          customerName: `${customer.first_name} ${customer.last_name}`,
          customerEmail: customer.email,
          workshopName: workshop.name,
          serviceDate: booking.appointment_date.toISOString().split('T')[0],
          reason: reason || 'Agendamento não disponível'
        })
      } catch (emailError) {
        console.error("Erro ao enviar email de rejeição:", emailError)
        // Não falhar a rejeição por erro de email
      }
    }

    return res.json({
      message: "Agendamento rejeitado",
      booking: {
        id: booking.id,
        status: 'recusado',
        cancelled_at: new Date().toISOString()
      }
    })

  } catch (error) {
    console.error("Erro ao rejeitar agendamento:", error)
    
    if (error.message.includes('Token')) {
      return res.status(401).json({
        message: "Não autorizado",
        error: error.message
      })
    }

    return res.status(500).json({
      message: "Erro ao rejeitar agendamento",
      error: error.message
    })
  }
}
