import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * GET /store/workshops/me/bank-account
 * 
 * Busca os dados bancários da oficina
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
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficina = oficinas[0]
    
    return res.json({
      success: true,
      data: {
        bank_account: oficina.metadata?.bank_account || null,
        is_configured: oficina.metadata?.bank_account != null
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar dados bancários:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar dados bancários",
      error: error.message
    })
  }
}

/**
 * PUT /store/workshops/me/bank-account
 * 
 * Atualiza os dados bancários da oficina
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { 
    bank_name,
    account_type,
    account_number,
    agency_number,
    account_holder_name,
    account_holder_document,
    pix_key,
    pix_key_type
  } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  // Validações básicas
  if (!bank_name || !account_type || !account_number || !agency_number || 
      !account_holder_name || !account_holder_document) {
    return res.status(400).json({
      message: "Todos os campos bancários são obrigatórios"
    })
  }
  
  if (!pix_key || !pix_key_type) {
    return res.status(400).json({
      message: "PIX é obrigatório"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    
    // Preparar dados bancários
    const bankAccountData = {
      bank_name,
      account_type, // 'checking' ou 'savings'
      account_number,
      agency_number,
      account_holder_name,
      account_holder_document,
      pix_key,
      pix_key_type, // 'cpf', 'cnpj', 'email', 'phone', 'random'
      updated_at: new Date().toISOString()
    }
    
    // Atualizar oficina com dados bancários no metadata
    const currentMetadata = oficinas[0].metadata || {}
    const updatedMetadata = {
      ...currentMetadata,
      bank_account: bankAccountData
    }
    
    const updatedOficina = await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      metadata: updatedMetadata
    }])
    
    return res.json({
      success: true,
      message: "Dados bancários atualizados com sucesso",
      data: {
        bank_account: bankAccountData
      }
    })
    
  } catch (error) {
    console.error("Erro ao atualizar dados bancários:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao atualizar dados bancários",
      error: error.message
    })
  }
}















