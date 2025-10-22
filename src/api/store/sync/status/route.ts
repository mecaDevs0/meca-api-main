/**
 * Endpoint de status de sincronização
 * GET /store/sync/status - Obter status de sincronização
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    // Contar registros em cada tabela
    const customerService = req.scope.resolve(Modules.CUSTOMER)
    const oficinaService = req.scope.resolve("oficinaModuleService")
    const vehicleService = req.scope.resolve("vehicleModuleService")
    const bookingService = req.scope.resolve("bookingModuleService")

    // Buscar contadores
    const [customers, workshops, vehicles, bookings] = await Promise.all([
      customerService.listCustomers({ take: 1 }),
      oficinaService.list({ take: 1 }),
      vehicleService.list({ take: 1 }),
      bookingService.list({ take: 1 })
    ])

    // Buscar dados mais recentes
    const [latestCustomers, latestWorkshops, latestVehicles, latestBookings] = await Promise.all([
      customerService.listCustomers({ 
        take: 1, 
        order: { updated_at: "DESC" } 
      }),
      oficinaService.list({ 
        take: 1, 
        order: { updated_at: "DESC" } 
      }),
      vehicleService.list({ 
        take: 1, 
        order: { updated_at: "DESC" } 
      }),
      bookingService.list({ 
        take: 1, 
        order: { updated_at: "DESC" } 
      })
    ])

    const status = {
      timestamp: new Date().toISOString(),
      database_status: 'connected',
      entities: {
        customers: {
          count: customers.length > 0 ? 'available' : 0,
          last_updated: latestCustomers[0]?.updated_at || null
        },
        workshops: {
          count: workshops.length > 0 ? 'available' : 0,
          last_updated: latestWorkshops[0]?.updated_at || null
        },
        vehicles: {
          count: vehicles.length > 0 ? 'available' : 0,
          last_updated: latestVehicles[0]?.updated_at || null
        },
        bookings: {
          count: bookings.length > 0 ? 'available' : 0,
          last_updated: latestBookings[0]?.updated_at || null
        }
      },
      sync_endpoints: {
        get_sync: '/store/sync',
        post_sync: '/store/sync',
        status: '/store/sync/status'
      },
      performance: {
        response_time: Date.now() - Date.now(),
        memory_usage: process.memoryUsage().heapUsed / 1024 / 1024, // MB
        uptime: process.uptime()
      }
    }

    return res.json(status)

  } catch (error) {
    console.error("Erro ao obter status de sincronização:", error)
    return res.status(500).json({
      message: "Erro ao obter status de sincronização",
      error: error.message
    })
  }
}
