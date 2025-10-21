import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * GET /health/ready
 * 
 * Kubernetes readiness probe
 * Verifica se a aplicação está pronta para receber tráfego
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    // Verifica se o banco está acessível
    const customerService = req.scope.resolve(Modules.CUSTOMER)
    await customerService.listCustomers({ take: 1 })
    
    return res.json({
      status: 'ready',
      timestamp: new Date().toISOString()
    })
  } catch (error) {
    return res.status(503).json({
      status: 'not ready',
      timestamp: new Date().toISOString(),
      error: error.message
    })
  }
}

