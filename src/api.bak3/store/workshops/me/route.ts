import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../modules/oficina"

/**
 * GET /store/workshops/me
 * 
 * Retorna os dados da oficina do usuário autenticado
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  // Por enquanto, buscaremos pela primeira oficina
  // Quando os links estiverem funcionando, usaremos o link user->oficina
  const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
  
  if (!oficinas || oficinas.length === 0) {
    return res.status(404).json({
      message: "Oficina não encontrada para este usuário"
    })
  }
  
  return res.json({ oficina: oficinas[0] })
}

/**
 * PUT /store/workshops/me
 * 
 * Atualiza os dados da oficina do usuário autenticado
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const {
    name,
    phone,
    address,
    description,
    logo_url,
    photo_urls,
    dados_bancarios,
    horario_funcionamento
  } = req.body
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }
    
    const oficinaId = oficinas[0].id
    
    // Atualizar oficina
    const updatedOficina = await oficinaModuleService.updateOficinas(oficinaId, {
      name,
      phone,
      address,
      description,
      logo_url,
      photo_urls,
      dados_bancarios,
      horario_funcionamento,
    })
    
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

