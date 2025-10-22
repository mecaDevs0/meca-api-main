import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"
import { OficinaStatus } from "../../../../../modules/oficina/models/oficina"

/**
 * POST /admin/workshops/:id/reject
 * 
 * Rejeita uma oficina pendente
 * 
 * Body:
 * - reason: Motivo da rejeição (obrigatório)
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const eventBusService = req.scope.resolve("eventBus")
  
  // Extrair ID da URL manualmente
  const url = req.url || ''
  const match = url.match(/\/workshops\/([^\/]+)\/reject/)
  const oficinaId = match ? match[1] : req.params.id || req.body.id
  const { reason } = req.body
  
  if (!reason) {
    return res.status(400).json({
      message: "O motivo da rejeição é obrigatório"
    })
  }
  
  try {
    // Buscar oficina
    const oficina = await oficinaModuleService.retrieveOficina(oficinaId)
    
    if (!oficina) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }
    
    // Verificar se está pendente
    if (oficina.status !== OficinaStatus.PENDENTE) {
      return res.status(400).json({
        message: `Esta oficina não pode ser rejeitada. Status atual: ${oficina.status}`
      })
    }
    
    // Rejeitar oficina (usar array conforme assinatura correta)
    const updatedOficinas = await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      status: OficinaStatus.REJEITADO,
      status_reason: reason,
    }])
    
    const updatedOficina = Array.isArray(updatedOficinas) ? updatedOficinas[0] : updatedOficinas
    
    // Emitir evento para notificar a oficina
    await eventBusService.emit("oficina.rejected", {
      oficina_id: oficinaId,
      oficina_email: oficina.email,
      oficina_name: oficina.name,
      reason,
    })
    
    return res.json({
      message: "Oficina rejeitada",
      oficina: updatedOficina
    })
    
  } catch (error) {
    console.error("Erro ao rejeitar oficina:", error)
    
    return res.status(500).json({
      message: "Erro ao rejeitar oficina",
      error: error.message
    })
  }
}

