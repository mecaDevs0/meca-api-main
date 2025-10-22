import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { PAGBANK_PAYMENT_MODULE } from "../../../modules/pagbank_payment"

export const AUTHENTICATE = false

/**
 * POST /public/payments
 * 
 * Processar pagamento (simulado)
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const paymentService = req.scope.resolve(PAGBANK_PAYMENT_MODULE)

  const {
    booking_id,
    order_id,
    amount,
    payment_method,
    card_info
  } = req.body

  if (!booking_id || !order_id || !amount || !payment_method) {
    return res.status(400).json({
      message: "Agendamento, pedido, valor e método de pagamento são obrigatórios"
    })
  }

  try {
    // Simular processamento de pagamento
    const payment = await paymentService.createPagbankPayments({
      order_id,
      amount: parseFloat(amount),
      payment_method,
      status: "approved", // Simular aprovação automática
      transaction_id: `sim_${Date.now()}`, // ID simulado
      metadata: {
        booking_id,
        card_last_4: card_info?.last_4 || "****",
        simulated: true
      }
    })

    return res.status(201).json({
      message: "Pagamento processado com sucesso!",
      payment: {
        id: payment.id,
        status: payment.status,
        transaction_id: payment.transaction_id,
        amount: payment.amount
      }
    })

  } catch (error) {
    console.error("Erro ao processar pagamento:", error)

    return res.status(500).json({
      message: "Erro ao processar pagamento",
      error: error.message
    })
  }
}

/**
 * GET /public/payments?order_id=xxx
 * 
 * Buscar pagamentos de um pedido
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const paymentService = req.scope.resolve(PAGBANK_PAYMENT_MODULE)
  const { order_id } = req.query

  if (!order_id) {
    return res.status(400).json({
      message: "ID do pedido é obrigatório"
    })
  }

  try {
    const payments = await paymentService.listPagbankPayments({ 
      order_id: order_id as string 
    })

    return res.json({
      payments,
      count: payments.length
    })

  } catch (error) {
    console.error("Erro ao buscar pagamentos:", error)

    return res.status(500).json({
      message: "Erro ao buscar pagamentos",
      error: error.message
    })
  }
}

