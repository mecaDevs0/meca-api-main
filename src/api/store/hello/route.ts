/**
 * Endpoint de teste simples
 * GET /store/hello
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  return res.json({
    message: "Hello from MECA API!",
    timestamp: new Date().toISOString(),
    status: "ok"
  })
}
