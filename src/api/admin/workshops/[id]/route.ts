import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../modules/oficina"

export const AUTHENTICATE = false

/**
 * PUT /admin/workshops/:id
 * 
 * Atualizar oficina
 */
export async function PUT(
  req: MedusaRequest<{ id: string }>,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const id = req.params.id
  
  try {
    const updatedOficinas = await oficinaModuleService.updateOficinas([{
      id: id,
      ...req.body
    }])
    
    const updatedOficina = Array.isArray(updatedOficinas) ? updatedOficinas[0] : updatedOficinas
    
    return res.json({
      message: "Oficina atualizada com sucesso",
      oficina: updatedOficina
    })
    
  } catch (error) {
    console.error("Erro ao atualizar oficina:", error)
    
    return res.status(500).json({
      message: "Erro ao atualizar oficina",
      error: error.message
    })
  }
}

/**
 * GET /admin/workshops/:id
 * 
 * Buscar oficina por ID
 */
export async function GET(
  req: MedusaRequest<{ id: string }>,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const id = req.params.id
  
  try {
    const oficina = await oficinaModuleService.retrieveOficina(id)
    
    if (!oficina) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }
    
    return res.json({
      oficina
    })
    
  } catch (error) {
    console.error("Erro ao buscar oficina:", error)
    
    return res.status(500).json({
      message: "Erro ao buscar oficina",
      error: error.message
    })
  }
}

