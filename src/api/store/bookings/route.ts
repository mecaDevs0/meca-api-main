import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../modules/booking"
import { VEHICLE_MODULE } from "../../../modules/vehicle"
import { OFICINA_MODULE } from "../../../modules/oficina"
import { BookingStatus } from "../../../modules/booking/models/booking"

/**
 * GET /store/bookings
 * 
 * Lista todos os agendamentos do cliente autenticado
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const customerId = req.auth_context?.actor_id
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    const { status, limit = 50, offset = 0 } = req.query
    
    const filters: any = {
      customer_id: customerId
    }
    
    if (status) {
      filters.status = status
    }
    
    const bookings = await bookingModuleService.listBookings(filters, {
      take: Number(limit),
      skip: Number(offset),
      order: { appointment_date: "DESC" }
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

/**
 * POST /store/bookings
 * 
 * Cria um novo agendamento (fluxo principal do cliente)
 * 
 * Body:
 * - vehicle_id: ID do veículo
 * - product_id: ID do serviço (Product)
 * - oficina_id: ID da oficina
 * - appointment_date: Data e hora do agendamento
 * - customer_notes: Observações do cliente (opcional)
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  const cartModuleService = req.scope.resolve(Modules.CART)
  const eventBusService = req.scope.resolve("eventBus")
  
  const customerId = req.auth_context?.actor_id
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const {
    vehicle_id,
    product_id,
    oficina_id,
    appointment_date,
    customer_notes
  } = req.body
  
  // Validações
  if (!vehicle_id || !product_id || !oficina_id || !appointment_date) {
    return res.status(400).json({
      message: "vehicle_id, product_id, oficina_id e appointment_date são obrigatórios"
    })
  }
  
  try {
    // 1. Validar que o veículo pertence ao cliente
    const vehicle = await vehicleModuleService.retrieveVehicle(vehicle_id)
    
    if (vehicle.customer_id !== customerId) {
      return res.status(403).json({
        message: "Este veículo não pertence a você"
      })
    }
    
    // 2. Validar que a oficina existe e está aprovada
    const oficina = await oficinaModuleService.retrieveOficina(oficina_id)
    
    if (oficina.status !== "aprovado") {
      return res.status(400).json({
        message: "Esta oficina não está disponível para agendamentos"
      })
    }
    
    // 3. Buscar o produto/serviço
    const product = await productModuleService.retrieveProduct(product_id, {
      relations: ["variants"]
    })
    
    if (!product || !product.variants || product.variants.length === 0) {
      return res.status(404).json({
        message: "Serviço não encontrado"
      })
    }
    
    const variant = product.variants[0]
    const price = variant.calculated_price?.amount || 0
    
    // 4. Criar snapshot do veículo
    const vehicleSnapshot = {
      marca: vehicle.marca,
      modelo: vehicle.modelo,
      ano: vehicle.ano,
      placa: vehicle.placa,
      km_atual: vehicle.km_atual
    }
    
    // 5. Criar o Booking
    const booking = await bookingModuleService.createBookings({
      customer_id: customerId,
      vehicle_id,
      oficina_id,
      product_id,
      appointment_date: new Date(appointment_date),
      status: BookingStatus.PENDENTE_OFICINA,
      vehicle_snapshot: vehicleSnapshot,
      customer_notes,
      estimated_price: price,
    })
    
    // 6. Criar Cart para este agendamento
    const cart = await cartModuleService.createCarts({
      currency_code: "brl",
      email: req.auth_context?.actor_email || "",
      metadata: {
        booking_id: booking.id,
        is_booking: true,
      }
    })
    
    // 7. Adicionar item ao carrinho
    await cartModuleService.addLineItems(cart.id, [{
      variant_id: variant.id,
      quantity: 1,
    }])
    
    // 8. Emitir evento para notificar a oficina
    await eventBusService.emit("booking.created", {
      booking_id: booking.id,
      customer_id: customerId,
      oficina_id,
      product_id,
      appointment_date,
    })
    
    return res.status(201).json({
      message: "Agendamento criado com sucesso! Aguardando confirmação da oficina.",
      booking,
      cart_id: cart.id,
    })
    
  } catch (error) {
    console.error("Erro ao criar agendamento:", error)
    
    return res.status(500).json({
      message: "Erro ao criar agendamento",
      error: error.message
    })
  }
}

