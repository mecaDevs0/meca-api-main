import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../../modules/oficina"
import { getPagBankService } from "../../../../../../services/pagbank"

/**
 * GET /store/workshops/me/pagbank/payments
 * 
 * Listar pagamentos recebidos pela oficina
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { limit = 50, offset = 0, start_date, end_date } = req.query
  
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
        message: "Oficina não possui conta PagBank configurada"
      })
    }
    
    // Buscar pagamentos no PagBank
    const pagBankService = getPagBankService()
    const paymentsResponse = await pagBankService.listPayments(
      pagbankAccountId,
      Number(limit),
      Number(offset)
    )
    
    if (!paymentsResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao buscar pagamentos",
        error: paymentsResponse.error
      })
    }
    
    // Filtrar por data se fornecido
    let payments = paymentsResponse.data || []
    
    if (start_date || end_date) {
      payments = payments.filter(payment => {
        const paymentDate = new Date(payment.created_at)
        const start = start_date ? new Date(start_date as string) : null
        const end = end_date ? new Date(end_date as string) : null
        
        if (start && paymentDate < start) return false
        if (end && paymentDate > end) return false
        
        return true
      })
    }
    
    // Calcular totais
    const totalAmount = payments.reduce((sum, payment) => sum + payment.amount, 0)
    const approvedPayments = payments.filter(p => p.status === 'approved')
    const pendingPayments = payments.filter(p => p.status === 'pending')
    const rejectedPayments = payments.filter(p => p.status === 'rejected')
    
    return res.json({
      success: true,
      data: {
        payments,
        summary: {
          total_amount: totalAmount,
          total_payments: payments.length,
          approved_payments: approvedPayments.length,
          pending_payments: pendingPayments.length,
          rejected_payments: rejectedPayments.length,
          approved_amount: approvedPayments.reduce((sum, p) => sum + p.amount, 0)
        },
        pagination: {
          limit: Number(limit),
          offset: Number(offset),
          total: payments.length
        }
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar pagamentos:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar pagamentos",
      error: error.message
    })
  }
}

/**
 * POST /store/workshops/me/pagbank/payments
 * 
 * Criar um pagamento de teste
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { amount, description, customer_id } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  if (!amount || !customer_id) {
    return res.status(400).json({
      message: "amount e customer_id são obrigatórios"
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
        message: "Oficina não possui conta PagBank configurada"
      })
    }
    
    // Criar pagamento no PagBank
    const pagBankService = getPagBankService()
    const paymentResponse = await pagBankService.createPayment({
      amount: Number(amount),
      currency: 'BRL',
      payment_method: 'pix', // PIX é o método mais comum no Brasil
      customer_id,
      description: description || `Pagamento para ${oficina.name}`
    })
    
    if (!paymentResponse.success) {
      return res.status(500).json({
        success: false,
        message: "Erro ao criar pagamento",
        error: paymentResponse.error
      })
    }
    
    return res.json({
      success: true,
      message: "Pagamento criado com sucesso",
      data: {
        payment: paymentResponse.data
      }
    })
    
  } catch (error) {
    console.error("Erro ao criar pagamento:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao criar pagamento",
      error: error.message
    })
  }
}














