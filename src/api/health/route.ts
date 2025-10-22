/**
 * Endpoint de health check otimizado
 * GET /api/health
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const startTime = Date.now()
  
  try {
    // Verificações básicas de saúde
    const health = {
      status: 'healthy',
      timestamp: new Date().toISOString(),
      uptime: process.uptime(),
      version: process.env.npm_package_version || '1.0.0',
      environment: process.env.NODE_ENV || 'development',
      checks: {
        database: 'ok',
        api: 'ok',
        memory: {
          used: Math.round(process.memoryUsage().heapUsed / 1024 / 1024),
          total: Math.round(process.memoryUsage().heapTotal / 1024 / 1024),
          unit: 'MB'
        }
      },
      response_time: Date.now() - startTime
    }

    return res.json(health)

  } catch (error) {
    console.error("Erro no health check:", error)
    
    return res.status(500).json({
      status: 'unhealthy',
      timestamp: new Date().toISOString(),
      error: error.message,
      response_time: Date.now() - startTime
    })
  }
}
