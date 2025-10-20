import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../modules/oficina"

export const AUTHENTICATE = false

/**
 * GET /admin/workshops
 * 
 * Lista todas as oficinas
 * Query params:
 * - status: filtrar por status (pendente, aprovado, rejeitado, suspenso)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const { status, limit = 50, offset = 0, q } = req.query
  
  const filters: any = {}
  
  if (status) {
    filters.status = status
  }
  
  if (q) {
    // Busca por nome ou CNPJ
    filters.$or = [
      { name: { $ilike: `%${q}%` } },
      { cnpj: { $ilike: `%${q}%` } }
    ]
  }
  
  try {
    const oficinas = await oficinaModuleService.listOficinas(filters, {
      take: Number(limit),
      skip: Number(offset),
      order: { created_at: "DESC" }
    })
    
    return res.json({
      oficinas,
      count: oficinas.length,
      limit: Number(limit),
      offset: Number(offset),
    })
    
  } catch (error) {
    console.error("Erro ao listar oficinas:", error)
    
    return res.status(500).json({
      message: "Erro ao listar oficinas",
      error: error.message
    })
  }
}

