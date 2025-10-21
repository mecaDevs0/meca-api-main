import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { getPagBankService } from "../../../../services/pagbank"

/**
 * POST /public/pagbank/webhook
 * 
 * Webhook para receber notificações do PagBank
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const payload = req.body
    const signature = req.headers['x-pagbank-signature'] as string
    
    console.log('PagBank Webhook recebido:', {
      event_type: payload.event_type,
      signature: signature ? 'present' : 'missing',
      timestamp: new Date().toISOString()
    })
    
    // Processar webhook com o serviço PagBank
    const pagBankService = getPagBankService()
    const webhookResponse = await pagBankService.handleWebhook(payload, signature)
    
    if (!webhookResponse.success) {
      console.error('Erro ao processar webhook PagBank:', webhookResponse.error)
      return res.status(400).json({
        success: false,
        message: "Erro ao processar webhook",
        error: webhookResponse.error
      })
    }
    
    // Processar eventos específicos
    await processWebhookEvent(payload)
    
    return res.json({
      success: true,
      message: "Webhook processado com sucesso",
      data: webhookResponse.data
    })
    
  } catch (error) {
    console.error("Erro no webhook PagBank:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro interno no webhook",
      error: error.message
    })
  }
}

/**
 * Processar eventos específicos do webhook
 */
async function processWebhookEvent(payload: any) {
  const eventType = payload.event_type
  const eventData = payload.data
  
  console.log(`Processando evento PagBank: ${eventType}`, eventData)
  
  switch (eventType) {
    case 'payment.approved':
      await handlePaymentApproved(eventData)
      break
    case 'payment.rejected':
      await handlePaymentRejected(eventData)
      break
    case 'payment.cancelled':
      await handlePaymentCancelled(eventData)
      break
    case 'account.updated':
      await handleAccountUpdated(eventData)
      break
    default:
      console.log(`Evento não tratado: ${eventType}`)
  }
}

/**
 * Pagamento aprovado
 */
async function handlePaymentApproved(eventData: any) {
  try {
    const paymentId = eventData.id
    const amount = eventData.amount
    const customerId = eventData.customer_id
    
    console.log(`Pagamento aprovado: ${paymentId} - R$ ${(amount / 100).toFixed(2)}`)
    
    // Aqui você pode:
    // 1. Atualizar status do agendamento
    // 2. Enviar notificação para a oficina
    // 3. Registrar no histórico financeiro
    // 4. Ativar serviços se necessário
    
    // Exemplo: buscar agendamento relacionado e atualizar status
    // const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
    // await bookingModuleService.updateBookings(bookingId, { status: 'paid' })
    
  } catch (error) {
    console.error('Erro ao processar pagamento aprovado:', error)
  }
}

/**
 * Pagamento rejeitado
 */
async function handlePaymentRejected(eventData: any) {
  try {
    const paymentId = eventData.id
    const reason = eventData.rejection_reason
    
    console.log(`Pagamento rejeitado: ${paymentId} - Motivo: ${reason}`)
    
    // Aqui você pode:
    // 1. Notificar o cliente sobre a rejeição
    // 2. Sugerir métodos de pagamento alternativos
    // 3. Atualizar status do agendamento
    
  } catch (error) {
    console.error('Erro ao processar pagamento rejeitado:', error)
  }
}

/**
 * Pagamento cancelado
 */
async function handlePaymentCancelled(eventData: any) {
  try {
    const paymentId = eventData.id
    
    console.log(`Pagamento cancelado: ${paymentId}`)
    
    // Aqui você pode:
    // 1. Liberar o agendamento para outros clientes
    // 2. Notificar a oficina sobre o cancelamento
    // 3. Atualizar status do agendamento
    
  } catch (error) {
    console.error('Erro ao processar pagamento cancelado:', error)
  }
}

/**
 * Conta atualizada
 */
async function handleAccountUpdated(eventData: any) {
  try {
    const accountId = eventData.id
    const status = eventData.status
    
    console.log(`Conta atualizada: ${accountId} - Status: ${status}`)
    
    // Aqui você pode:
    // 1. Atualizar status da conta no banco de dados
    // 2. Notificar a oficina sobre mudanças
    // 3. Ativar/desativar funcionalidades baseado no status
    
  } catch (error) {
    console.error('Erro ao processar conta atualizada:', error)
  }
}














