/**
 * Endpoint de teste simples
 * GET /api/test
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
    status: "ok"
  })
}
