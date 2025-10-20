import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"
import { OficinaStatus } from "../../../../../modules/oficina/models/oficina"

export const AUTHENTICATE = false

/**
 * POST /admin/workshops/:id/approve
 * 
 * Aprova uma oficina pendente
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  // Extrair ID da URL manualmente
  const url = req.url || ''
  const match = url.match(/\/workshops\/([^\/]+)\/approve/)
  const oficinaId = match ? match[1] : req.params.id || req.body.id
  
  if (!oficinaId) {
    return res.status(400).json({
      message: "ID da oficina é obrigatório",
      url: req.url
    })
  }
  
  try {
    // Buscar oficina
    const oficina = await oficinaModuleService.retrieveOficina(oficinaId)
    
    if (!oficina) {
      return res.status(404).json({
        message: "Oficina não encontrada",
        id: oficinaId
      })
    }
    
    // A assinatura correta do updateOficinas é: updateOficinas(data: UpdateDTO[])
    // Onde cada objeto no array deve ter um 'id' e os campos a serem atualizados
    const updatedOficinas = await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      status: OficinaStatus.APROVADO
    }])
    
    const updatedOficina = Array.isArray(updatedOficinas) ? updatedOficinas[0] : updatedOficinas
    
    return res.json({
      message: "Oficina aprovada com sucesso",
      oficina: updatedOficina
    })
    
  } catch (error) {
    console.error("Erro ao aprovar oficina:", error)
    
    return res.status(500).json({
      message: "Erro ao aprovar oficina",
      error: error.message
    })
  }
}
