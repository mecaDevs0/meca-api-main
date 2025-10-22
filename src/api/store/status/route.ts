/**
 * Endpoint de status simples
 * GET /store/status
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  return res.json({
    status: "ok",
    timestamp: new Date().toISOString(),
    message: "MECA API funcionando!",
    endpoints: {
      sync: "/store/sync",
      cache: "/store/cache",
      register: "/store/register",
      login: "/store/login"
    }
  })
}
