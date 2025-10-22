import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"

export const AUTHENTICATE = false

/**
 * GET /health/live
 * 
 * Kubernetes liveness probe
 * Verifica se a aplicação está viva (processo rodando)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  return res.json({
    status: 'alive',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    memory: {
      rss: `${Math.round(process.memoryUsage().rss / 1024 / 1024)}MB`,
      heapTotal: `${Math.round(process.memoryUsage().heapTotal / 1024 / 1024)}MB`,
      heapUsed: `${Math.round(process.memoryUsage().heapUsed / 1024 / 1024)}MB`,
      external: `${Math.round(process.memoryUsage().external / 1024 / 1024)}MB`
    },
    pid: process.pid,
    node_version: process.version
  })
}


