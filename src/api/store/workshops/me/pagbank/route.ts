import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"
import { getPagBankService } from "../../../../../services/pagbank"

/**
 * GET /store/workshops/me/pagbank
 * 
 * Buscar informações da conta PagBank da oficina
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
      return res.json({
        success: true,
        data: {
          has_account: false,
          account: null,
          bank_accounts: []
        }
      })
    }
    
    // Buscar informações da conta no PagBank
    const pagBankService = getPagBankService()
    const accountResponse = await pagBankService.getAccount(pagbankAccountId)
    
    if (!accountResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao buscar conta PagBank",
        error: accountResponse.error
      })
    }
    
    // Buscar contas bancárias
    const bankAccountsResponse = await pagBankService.getBankAccounts(pagbankAccountId)
    
    return res.json({
      success: true,
      data: {
        has_account: true,
        account: accountResponse.data,
        bank_accounts: bankAccountsResponse.data || []
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar informações PagBank:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar informações PagBank",
      error: error.message
    })
  }
}

/**
 * POST /store/workshops/me/pagbank
 * 
 * Criar conta no PagBank para a oficina
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { 
    name,
    email,
    document,
    type,
    address,
    phone
  } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  // Validações
  if (!name || !email || !document || !type || !address || !phone) {
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
    
    const oficinaId = oficinas[0].id
    const oficina = oficinas[0]
    
    // Verificar se já tem conta PagBank
    const metadata = oficina.metadata || {}
    if (metadata.pagbank_account_id) {
      return res.status(400).json({
        message: "Oficina já possui conta PagBank"
      })
    }
    
    // Criar conta no PagBank
    const pagBankService = getPagBankService()
    const accountResponse = await pagBankService.createAccount({
      name,
      email,
      document,
      type,
      address,
      phone
    })
    
    if (!accountResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao criar conta PagBank",
        error: accountResponse.error
      })
    }
    
    // Salvar ID da conta no metadata da oficina
    const updatedMetadata = {
      ...metadata,
      pagbank_account_id: accountResponse.data?.id,
      pagbank_account_created_at: new Date().toISOString()
    }
    
    await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      metadata: updatedMetadata
    }])
    
    return res.json({
      success: true,
      message: "Conta PagBank criada com sucesso",
      data: {
        account: accountResponse.data
      }
    })
    
  } catch (error) {
    console.error("Erro ao criar conta PagBank:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao criar conta PagBank",
      error: error.message
    })
  }
}

/**
 * PUT /store/workshops/me/pagbank
 * 
 * Atualizar informações da conta PagBank
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const updateData = req.body
  
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
    
    // Atualizar conta no PagBank
    const pagBankService = getPagBankService()
    const accountResponse = await pagBankService.updateAccount(pagbankAccountId, updateData)
    
    if (!accountResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao atualizar conta PagBank",
        error: accountResponse.error
      })
    }
    
    return res.json({
      success: true,
      message: "Conta PagBank atualizada com sucesso",
      data: {
        account: accountResponse.data
      }
    })
    
  } catch (error) {
    console.error("Erro ao atualizar conta PagBank:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao atualizar conta PagBank",
      error: error.message
    })
  }
}














