/**
 * Endpoint de teste na rota store
 * GET /store/test
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  return res.json({
    message: "API funcionando!",
    timestamp: new Date().toISOString(),
    status: "ok",
    endpoints: {
      health: "/api/health",
      customer_register: "/api/customer/register",
      customer_login: "/api/customer/login",
      workshop_register: "/api/workshop/register",
      workshop_login: "/api/workshop/login"
    }
  })
}
