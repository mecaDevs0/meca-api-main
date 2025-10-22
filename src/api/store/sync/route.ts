/**
 * Endpoint de sincronização de dados
 * GET /store/sync - Obter dados para sincronização
 * POST /store/sync - Enviar dados para sincronização
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * GET /store/sync
 * Obter dados para sincronização
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const { last_sync, entity_type } = req.query

    // Buscar dados baseado no tipo de entidade
    let syncData = {}

    switch (entity_type) {
      case 'customers':
        const customerService = req.scope.resolve(Modules.CUSTOMER)
        const customers = await customerService.listCustomers({
          take: 100,
          order: { updated_at: "DESC" }
        })
        syncData = {
          customers: customers.map(customer => ({
            id: customer.id,
            email: customer.email,
            first_name: customer.first_name,
            last_name: customer.last_name,
            phone: customer.phone,
            metadata: customer.metadata,
            created_at: customer.created_at,
            updated_at: customer.updated_at
          }))
        }
        break

      case 'workshops':
        const oficinaService = req.scope.resolve("oficinaModuleService")
        const workshops = await oficinaService.list({
          take: 100,
          order: { updated_at: "DESC" }
        })
        syncData = {
          workshops: workshops.map(workshop => ({
            id: workshop.id,
            name: workshop.name,
            email: workshop.email,
            phone: workshop.phone,
            status: workshop.status,
            address: workshop.address,
            metadata: workshop.metadata,
            created_at: workshop.created_at,
            updated_at: workshop.updated_at
          }))
        }
        break

      case 'vehicles':
        const vehicleService = req.scope.resolve("vehicleModuleService")
        const vehicles = await vehicleService.list({
          take: 100,
          order: { updated_at: "DESC" }
        })
        syncData = {
          vehicles: vehicles.map(vehicle => ({
            id: vehicle.id,
            marca: vehicle.marca,
            modelo: vehicle.modelo,
            ano: vehicle.ano,
            placa: vehicle.placa,
            cor: vehicle.cor,
            km_atual: vehicle.km_atual,
            combustivel: vehicle.combustivel,
            observacoes: vehicle.observacoes,
            customer_id: vehicle.customer_id,
            created_at: vehicle.created_at,
            updated_at: vehicle.updated_at
          }))
        }
        break

      case 'bookings':
        const bookingService = req.scope.resolve("bookingModuleService")
        const bookings = await bookingService.list({
          take: 100,
          order: { updated_at: "DESC" }
        })
        syncData = {
          bookings: bookings.map(booking => ({
            id: booking.id,
            appointment_date: booking.appointment_date,
            status: booking.status,
            customer_id: booking.customer_id,
            vehicle_id: booking.vehicle_id,
            oficina_id: booking.oficina_id,
            customer_notes: booking.customer_notes,
            oficina_notes: booking.oficina_notes,
            estimated_price: booking.estimated_price,
            final_price: booking.final_price,
            confirmed_at: booking.confirmed_at,
            completed_at: booking.completed_at,
            created_at: booking.created_at,
            updated_at: booking.updated_at
          }))
        }
        break

      default:
        return res.status(400).json({
          message: "Tipo de entidade inválido. Use: customers, workshops, vehicles, bookings"
        })
    }

    return res.json({
      success: true,
      entity_type,
      last_sync: new Date().toISOString(),
      data: syncData,
      count: Object.values(syncData)[0]?.length || 0
    })

  } catch (error) {
    console.error("Erro na sincronização:", error)
    return res.status(500).json({
      message: "Erro na sincronização",
      error: error.message
    })
  }
}

/**
 * POST /store/sync
 * Enviar dados para sincronização
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const { entity_type, data } = req.body

    if (!entity_type || !data) {
      return res.status(400).json({
        message: "entity_type e data são obrigatórios"
      })
    }

    let syncResult = {}

    switch (entity_type) {
      case 'customers':
        const customerService = req.scope.resolve(Modules.CUSTOMER)
        const customerResults = []
        
        for (const customerData of data) {
          try {
            // Verificar se cliente já existe
            const existingCustomers = await customerService.listCustomers({
              id: customerData.id,
              take: 1
            })

            if (existingCustomers.length > 0) {
              // Atualizar cliente existente
              const updated = await customerService.updateCustomers(customerData.id, {
                email: customerData.email,
                first_name: customerData.first_name,
                last_name: customerData.last_name,
                phone: customerData.phone,
                metadata: customerData.metadata
              })
              customerResults.push({ id: customerData.id, action: 'updated', data: updated })
            } else {
              // Criar novo cliente
              const created = await customerService.createCustomers({
                id: customerData.id,
                email: customerData.email,
                first_name: customerData.first_name,
                last_name: customerData.last_name,
                phone: customerData.phone,
                metadata: customerData.metadata
              })
              customerResults.push({ id: customerData.id, action: 'created', data: created })
            }
          } catch (error) {
            customerResults.push({ id: customerData.id, action: 'error', error: error.message })
          }
        }
        
        syncResult = { customers: customerResults }
        break

      case 'vehicles':
        const vehicleService = req.scope.resolve("vehicleModuleService")
        const vehicleResults = []
        
        for (const vehicleData of data) {
          try {
            // Verificar se veículo já existe
            const existingVehicles = await vehicleService.list({
              id: vehicleData.id,
              take: 1
            })

            if (existingVehicles.length > 0) {
              // Atualizar veículo existente
              const updated = await vehicleService.update(vehicleData.id, {
                marca: vehicleData.marca,
                modelo: vehicleData.modelo,
                ano: vehicleData.ano,
                placa: vehicleData.placa,
                cor: vehicleData.cor,
                km_atual: vehicleData.km_atual,
                combustivel: vehicleData.combustivel,
                observacoes: vehicleData.observacoes,
                customer_id: vehicleData.customer_id
              })
              vehicleResults.push({ id: vehicleData.id, action: 'updated', data: updated })
            } else {
              // Criar novo veículo
              const created = await vehicleService.create({
                id: vehicleData.id,
                marca: vehicleData.marca,
                modelo: vehicleData.modelo,
                ano: vehicleData.ano,
                placa: vehicleData.placa,
                cor: vehicleData.cor,
                km_atual: vehicleData.km_atual,
                combustivel: vehicleData.combustivel,
                observacoes: vehicleData.observacoes,
                customer_id: vehicleData.customer_id
              })
              vehicleResults.push({ id: vehicleData.id, action: 'created', data: created })
            }
          } catch (error) {
            vehicleResults.push({ id: vehicleData.id, action: 'error', error: error.message })
          }
        }
        
        syncResult = { vehicles: vehicleResults }
        break

      default:
        return res.status(400).json({
          message: "Tipo de entidade inválido para sincronização"
        })
    }

    return res.json({
      success: true,
      entity_type,
      sync_result: syncResult,
      timestamp: new Date().toISOString()
    })

  } catch (error) {
    console.error("Erro na sincronização de dados:", error)
    return res.status(500).json({
      message: "Erro na sincronização de dados",
      error: error.message
    })
  }
}
