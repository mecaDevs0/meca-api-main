import { MedusaContainer } from "@medusajs/framework"
import { CacheService } from "../services/cache"

/**
 * Scheduled Job: Data Synchronization
 * 
 * Sincroniza dados entre cache e banco
 * Executa a cada 5 minutos
 */

export default async function syncDataJob(container: MedusaContainer) {
  const logger = container.resolve("logger")
  const eventBus = container.resolve("eventBus")
  
  try {
    logger.info("Starting data synchronization job")
    
    // Limpar caches expirados
    CacheService.cleanExpired()
    
    // Emitir evento para subscribers sincronizarem dados
    await eventBus.emit("data.sync.requested", {
      timestamp: new Date().toISOString(),
      source: "scheduled-job"
    })
    
    logger.info("Data synchronization completed")
    
    return {
      success: true,
      timestamp: new Date().toISOString()
    }
  } catch (error) {
    logger.error("Error in data sync job:", error)
    throw error
  }
}

export const config = {
  name: "sync-data",
  schedule: "*/5 * * * *", // A cada 5 minutos
  data: {},
}


