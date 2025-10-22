/**
 * Endpoints para agendamentos de clientes
 * GET /api/customer/bookings - Listar agendamentos do cliente
 * POST /api/customer/bookings - Criar novo agendamento
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"
import EmailService from "../../../services/email"

export const AUTHENTICATE = false

// Middleware de autenticação JWT
async function authenticateCustomer(req: MedusaRequest): Promise<any> {
  const authHeader = req.headers.authorization
  if (!authHeader || !authHeader.startsWith('Bearer ')) {
    throw new Error('Token de autenticação necessário')
  }

  const token = authHeader.substring(7)
  const { AuthService } = await import("../../../services/auth")
  
  try {
    const decoded = AuthService.verifyToken(token) as any
    if (decoded.type !== 'customer') {
      throw new Error('Token inválido para cliente')
    }
    return decoded
  } catch (error) {
    throw new Error('Token inválido')
  }
}

/**
 * GET /api/customer/bookings
 * Listar agendamentos do cliente
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const customer = await authenticateCustomer(req)
    const bookingService = req.scope.resolve("bookingModuleService")

    const bookings = await bookingService.list({
      customer_id: customer.id,
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
    console.error("Erro ao listar agendamentos:", error)
    return res.status(401).json({
      message: "Não autorizado",
      error: error.message
    })
  }
}

/**
 * POST /api/customer/bookings
 * Criar novo agendamento
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const customer = await authenticateCustomer(req)
    const { vehicle_id, oficina_id, appointment_date, customer_notes, service_type } = req.body

    if (!vehicle_id || !oficina_id || !appointment_date) {
      return res.status(400).json({
        message: "vehicle_id, oficina_id e appointment_date são obrigatórios"
      })
    }

    const bookingService = req.scope.resolve("bookingModuleService")

    // Criar agendamento
    const booking = await bookingService.create({
      customer_id: customer.id,
      vehicle_id,
      oficina_id,
      appointment_date: new Date(appointment_date),
      status: 'pendente_oficina',
      customer_notes,
      vehicle_snapshot: {
        service_type,
        requested_at: new Date().toISOString()
      }
    })

    // Buscar dados da oficina para email
    const oficinaService = req.scope.resolve("oficinaModuleService")
    const oficinas = await oficinaService.list({
      id: oficina_id,
      take: 1
    })

    if (oficinas.length > 0) {
      const oficina = oficinas[0]
      
      // Enviar notificação para oficina (opcional)
      try {
        // Aqui você pode implementar notificação para oficina
        console.log(`Novo agendamento criado para oficina ${oficina.name}`)
      } catch (emailError) {
        console.error("Erro ao notificar oficina:", emailError)
      }
    }

    return res.status(201).json({
      message: "Agendamento criado com sucesso!",
      booking: {
        id: booking.id,
        appointment_date: booking.appointment_date,
        status: booking.status,
        customer_notes: booking.customer_notes
      }
    })

  } catch (error) {
    console.error("Erro ao criar agendamento:", error)
    
    if (error.message.includes('Token')) {
      return res.status(401).json({
        message: "Não autorizado",
        error: error.message
      })
    }

    return res.status(500).json({
      message: "Erro ao criar agendamento",
      error: error.message
    })
  }
}
