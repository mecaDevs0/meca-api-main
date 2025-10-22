import { MedusaContainer } from "@medusajs/framework"
import { Modules } from "@medusajs/framework/utils"

/**
 * Scheduled Job: Cleanup Expired Tokens
 * 
 * Remove tokens de recuperação de senha expirados
 * Executa diariamente às 3:00 AM
 */

export default async function cleanupExpiredTokensJob(container: MedusaContainer) {
  const logger = container.resolve("logger")
  const customerService = container.resolve(Modules.CUSTOMER)
  
  try {
    logger.info("Starting cleanup of expired password reset tokens")
    
    const now = new Date()
    let cleaned = 0
    
    // Buscar todos os clientes com tokens de reset
    const customers = await customerService.listCustomers({
      filters: {
        metadata: {
          reset_token_expiry: {
            $ne: null
          }
        }
      }
    })
    
    // Limpar tokens expirados
    for (const customer of customers) {
      const tokenExpiry = customer.metadata?.reset_token_expiry as string
      
      if (tokenExpiry && new Date(tokenExpiry) < now) {
        await customerService.updateCustomers(customer.id, {
          metadata: {
            ...customer.metadata,
            reset_token: null,
            reset_token_expiry: null
          }
        })
        cleaned++
      }
    }
    
    logger.info(`Cleanup completed: ${cleaned} expired tokens removed from customers`)
    
    // TODO: Fazer o mesmo para oficinas quando tiver o módulo
    
    return {
      success: true,
      cleaned,
      timestamp: new Date().toISOString()
    }
  } catch (error) {
    logger.error("Error in cleanup expired tokens job:", error)
    throw error
  }
}

export const config = {
  name: "cleanup-expired-tokens",
  schedule: "0 3 * * *", // Diariamente às 3:00 AM
  data: {},
}


