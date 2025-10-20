import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../../modules/oficina"
import { getPagBankService } from "../../../../../../services/pagbank"

/**
 * GET /store/workshops/me/pagbank/bank-accounts
 * 
 * Buscar contas bancárias da oficina no PagBank
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
    const metadata = oficina.metadata || {}
    const pagbankAccountId = metadata.pagbank_account_id
    
    if (!pagbankAccountId) {
      return res.status(400).json({
        message: "Oficina não possui conta PagBank"
      })
    }
    
    // Buscar contas bancárias no PagBank
    const pagBankService = getPagBankService()
    const bankAccountsResponse = await pagBankService.getBankAccounts(pagbankAccountId)
    
    if (!bankAccountsResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao buscar contas bancárias",
        error: bankAccountsResponse.error
      })
    }
    
    return res.json({
      success: true,
      data: {
        bank_accounts: bankAccountsResponse.data || []
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar contas bancárias:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar contas bancárias",
      error: error.message
    })
  }
}

/**
 * POST /store/workshops/me/pagbank/bank-accounts
 * 
 * Adicionar conta bancária no PagBank
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const {
    account_number,
    bank_code,
    agency_number,
    holder_name,
    holder_type
  } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  // Validações
  if (!account_number || !bank_code || !agency_number || !holder_name || !holder_type) {
    return res.status(400).json({
      message: "Todos os campos são obrigatórios"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficina = oficinas[0]
    const metadata = oficina.metadata || {}
    const pagbankAccountId = metadata.pagbank_account_id
    
    if (!pagbankAccountId) {
      return res.status(400).json({
        message: "Oficina não possui conta PagBank"
      })
    }
    
    // Criar conta bancária no PagBank
    const pagBankService = getPagBankService()
    const bankAccountResponse = await pagBankService.createBankAccount(pagbankAccountId, {
      account_number,
      bank_code,
      agency_number,
      holder_name,
      holder_type
    })
    
    if (!bankAccountResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao criar conta bancária",
        error: bankAccountResponse.error
      })
    }
    
    return res.json({
      success: true,
      message: "Conta bancária criada com sucesso",
      data: {
        bank_account: bankAccountResponse.data
      }
    })
    
  } catch (error) {
    console.error("Erro ao criar conta bancária:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao criar conta bancária",
      error: error.message
    })
  }
}




