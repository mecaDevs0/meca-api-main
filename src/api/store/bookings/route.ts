import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../modules/booking"

export const AUTHENTICATE = true

/**
 * GET /store/bookings
 *
 * Listar agendamentos do cliente logado
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  
  try {
    const bookings = await bookingModuleService.listBookings({
      customer_id: req.auth_context.customer_id
    })

    const formattedBookings = bookings.map(booking => ({
      id: booking.id,
      workshop_id: booking.workshop_id,
      workshop_name: booking.workshop_name,
      vehicle_id: booking.vehicle_id,
      vehicle_plate: booking.vehicle_plate,
      service_ids: booking.service_ids,
      service_names: booking.service_names,
      scheduled_date: booking.scheduled_date,
      scheduled_time: booking.scheduled_time,
      status: booking.status,
      total_price: booking.total_price,
      notes: booking.notes,
      created_at: booking.created_at,
      updated_at: booking.updated_at
    }))

    return res.json({
      bookings: formattedBookings,
      total: formattedBookings.length
    })

  } catch (error) {
    console.error("Erro ao buscar agendamentos:", error)
    return res.status(500).json({
      message: "Erro ao buscar agendamentos",
      error: error.message
    })
  }
}

/**
 * POST /store/bookings
 *
 * Criar novo agendamento
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const oficinaModuleService = req.scope.resolve(Modules.OFICINA)
  const vehicleModuleService = req.scope.resolve(Modules.VEHICLE)
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  
  const {
    workshop_id,
    vehicle_id,
    service_ids,
    scheduled_date,
    scheduled_time,
    notes
  } = req.body

  // Validações básicas
  if (!workshop_id || !vehicle_id || !service_ids || !scheduled_date || !scheduled_time) {
    return res.status(400).json({
      message: "Todos os campos obrigatórios devem ser preenchidos"
    })
  }

  try {
    // Verificar se a oficina existe e está aprovada
    const workshop = await oficinaModuleService.retrieveOficina(workshop_id)
    if (!workshop || workshop.status !== "aprovado") {
      return res.status(404).json({
        message: "Oficina não encontrada ou não aprovada"
      })
    }

    // Verificar se o veículo pertence ao cliente
    const vehicle = await vehicleModuleService.retrieveVehicle(vehicle_id)
    if (!vehicle || vehicle.customer_id !== req.auth_context.customer_id) {
      return res.status(404).json({
        message: "Veículo não encontrado"
      })
    }

    // Verificar se os serviços existem e calcular preço total
    let totalPrice = 0
    const serviceNames = []
    
    for (const serviceId of service_ids) {
      const service = await productModuleService.retrieveProduct(serviceId)
      if (!service) {
        return res.status(404).json({
          message: `Serviço ${serviceId} não encontrado`
        })
      }
      totalPrice += service.price || 0
      serviceNames.push(service.title)
    }

    // Adicionar taxa MECA de 5%
    const mecaFee = totalPrice * 0.05
    const finalPrice = totalPrice + mecaFee

    const booking = await bookingModuleService.createBookings({
      customer_id: req.auth_context.customer_id,
      workshop_id,
      workshop_name: workshop.name,
      vehicle_id,
      vehicle_plate: vehicle.plate,
      service_ids,
      service_names: serviceNames,
      scheduled_date,
      scheduled_time,
      status: "pendente",
      total_price: finalPrice,
      meca_fee: mecaFee,
      notes
    })

    return res.status(201).json({
      message: "Agendamento criado com sucesso",
      booking: {
        id: booking.id,
        workshop_id: booking.workshop_id,
        workshop_name: booking.workshop_name,
        vehicle_id: booking.vehicle_id,
        vehicle_plate: booking.vehicle_plate,
        service_ids: booking.service_ids,
        service_names: booking.service_names,
        scheduled_date: booking.scheduled_date,
        scheduled_time: booking.scheduled_time,
        status: booking.status,
        total_price: booking.total_price,
        meca_fee: booking.meca_fee,
        notes: booking.notes,
        created_at: booking.created_at
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


