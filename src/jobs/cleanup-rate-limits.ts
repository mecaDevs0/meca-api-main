import { MedusaContainer } from "@medusajs/framework"
import { RateLimiter } from "../api/middlewares/rate-limit"
import { CacheService } from "../services/cache"

/**
 * Scheduled Job: Cleanup Rate Limits
 * 
 * Limpa entradas expiradas de rate limiting e cache
 * Executa a cada hora
 */

export default async function cleanupRateLimitsJob(container: MedusaContainer) {
  const logger = container.resolve("logger")
  
  try {
    logger.info("Starting cleanup of expired rate limits and caches")
    
    // Limpar rate limits expirados
    RateLimiter.cleanExpired()
    
    // Limpar caches expirados
    CacheService.cleanExpired()
    
    logger.info("Cleanup completed successfully")
    
    return {
      success: true,
      timestamp: new Date().toISOString()
    }
  } catch (error) {
    logger.error("Error in cleanup job:", error)
    throw error
  }
}

export const config = {
  name: "cleanup-rate-limits",
  schedule: "0 * * * *", // A cada hora
  data: {},
}

