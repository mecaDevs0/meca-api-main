import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../modules/booking"

export const AUTHENTICATE = false

/**
 * POST /public/bookings
 * 
 * Criar novo agendamento
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingService = req.scope.resolve(BOOKING_MODULE)
  const orderService = req.scope.resolve(Modules.ORDER)

  const {
    customer_id,
    oficina_id,
    vehicle_id,
    service_id,
    scheduled_date,
    scheduled_time,
    notes
  } = req.body

  if (!customer_id || !oficina_id || !vehicle_id || !service_id || !scheduled_date) {
    return res.status(400).json({
      message: "Cliente, oficina, veículo, serviço e data são obrigatórios"
    })
  }

  try {
    // Criar pedido primeiro
    const order = await orderService.createOrders({
      currency_code: "BRL",
      email: "", // Será preenchido depois
      metadata: {
        customer_id,
        oficina_id,
        vehicle_id,
        service_id
      }
    })

    // Criar agendamento
    const booking = await bookingService.createBookings({
      customer_id,
      oficina_id,
      vehicle_id,
      order_id: order.id,
      scheduled_date: new Date(scheduled_date),
      scheduled_time,
      status: "pendente",
      notes
    })

    return res.status(201).json({
      message: "Agendamento criado com sucesso!",
      booking: {
        id: booking.id,
        scheduled_date: booking.scheduled_date,
        scheduled_time: booking.scheduled_time,
        status: booking.status,
        order_id: order.id
      }
    })

  } catch (error) {
    console.error("Erro ao criar agendamento:", error)

    return res.status(500).json({
      message: "Erro ao criar agendamento",
      error: error.message
    })
  }
}

/**
 * GET /public/bookings?customer_id=xxx
 * 
 * Lista agendamentos do cliente
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingService = req.scope.resolve(BOOKING_MODULE)
  const { customer_id, oficina_id } = req.query

  const filters: any = {}

  if (customer_id) {
    filters.customer_id = customer_id as string
  }

  if (oficina_id) {
    filters.oficina_id = oficina_id as string
  }

  try {
    const bookings = await bookingService.listBookings(filters, {
      order: { scheduled_date: "DESC" }
    })

    return res.json({
      bookings,
      count: bookings.length
    })

  } catch (error) {
    console.error("Erro ao listar agendamentos:", error)

    return res.status(500).json({
      message: "Erro ao listar agendamentos",
      error: error.message
    })
  }
}

