import { SubscriberArgs, SubscriberConfig } from "@medusajs/framework"

/**
 * Subscriber: Data Synchronization
 * 
 * Mantém dados sincronizados entre as 3 frentes (admin, app cliente, app oficina)
 * Emite eventos para invalidar caches e atualizar dados em tempo real
 */

export default async function dataSyncHandler({
  event,
  container,
}: SubscriberArgs<any>) {
  const { data, name: event_name } = event
  const logger = container.resolve("logger")
  const eventBus = container.resolve("eventBus")
  
  try {
    // Log para auditoria
    logger.info(`Data sync event: ${event_name}`, { data })
    
    // Invalidar caches relacionados baseado no tipo de evento
    switch (event_name) {
      case "booking.created":
      case "booking.updated":
      case "booking.deleted":
        await invalidateBookingCaches(data, container, eventBus)
        break
        
      case "workshop.created":
      case "workshop.updated":
      case "workshop.approved":
      case "workshop.rejected":
        await invalidateWorkshopCaches(data, container, eventBus)
        break
        
      case "customer.created":
      case "customer.updated":
        await invalidateCustomerCaches(data, container, eventBus)
        break
        
      case "vehicle.created":
      case "vehicle.updated":
      case "vehicle.deleted":
        await invalidateVehicleCaches(data, container, eventBus)
        break
    }
  } catch (error) {
    logger.error(`Data sync error for ${event_name}:`, error)
  }
}

async function invalidateBookingCaches(data: any, container: any, eventBus: any) {
  const logger = container.resolve("logger")
  
  try {
    // Emitir evento para apps atualizarem dados
    await eventBus.emit("cache.invalidate", {
      type: "booking",
      id: data.id || data.booking_id,
      customer_id: data.customer_id,
      workshop_id: data.oficina_id || data.workshop_id
    })
    
    logger.debug(`Booking cache invalidated for booking ${data.id || data.booking_id}`)
  } catch (error) {
    logger.error("Failed to invalidate booking cache:", error)
  }
}

async function invalidateWorkshopCaches(data: any, container: any, eventBus: any) {
  const logger = container.resolve("logger")
  
  try {
    // Emitir evento para apps atualizarem lista de oficinas
    await eventBus.emit("cache.invalidate", {
      type: "workshop",
      id: data.id || data.workshop_id
    })
    
    logger.debug(`Workshop cache invalidated for workshop ${data.id || data.workshop_id}`)
  } catch (error) {
    logger.error("Failed to invalidate workshop cache:", error)
  }
}

async function invalidateCustomerCaches(data: any, container: any, eventBus: any) {
  const logger = container.resolve("logger")
  
  try {
    // Emitir evento para apps atualizarem dados do cliente
    await eventBus.emit("cache.invalidate", {
      type: "customer",
      id: data.id || data.customer_id
    })
    
    logger.debug(`Customer cache invalidated for customer ${data.id || data.customer_id}`)
  } catch (error) {
    logger.error("Failed to invalidate customer cache:", error)
  }
}

async function invalidateVehicleCaches(data: any, container: any, eventBus: any) {
  const logger = container.resolve("logger")
  
  try {
    // Emitir evento para apps atualizarem dados de veículos
    await eventBus.emit("cache.invalidate", {
      type: "vehicle",
      id: data.id || data.vehicle_id,
      customer_id: data.customer_id
    })
    
    logger.debug(`Vehicle cache invalidated for vehicle ${data.id || data.vehicle_id}`)
  } catch (error) {
    logger.error("Failed to invalidate vehicle cache:", error)
  }
}

export const config: SubscriberConfig = {
  event: [
    "booking.*",
    "workshop.*",
    "customer.*",
    "vehicle.*"
  ],
}

