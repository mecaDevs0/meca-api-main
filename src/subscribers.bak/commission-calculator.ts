import { SubscriberArgs, SubscriberConfig } from "@medusajs/framework"
import { Modules } from "@medusajs/framework/utils"

/**
 * Subscriber: Commission Calculator
 * 
 * Calcula a comissão da MECA quando um pedido é completado
 * Escuta o evento: order.placed
 */

export default async function handleOrderPlaced({
  event: { data },
  container,
}: SubscriberArgs<{ id: string }>) {
  const orderModuleService = container.resolve(Modules.ORDER)
  const logger = container.resolve("logger")
  
  try {
    const orderId = data.id
    
    // Buscar detalhes completos do pedido
    const order = await orderModuleService.retrieveOrder(orderId, {
      relations: ["items"]
    })
    
    if (!order) {
      logger.warn(`Order ${orderId} não encontrado para cálculo de comissão`)
      return
    }
    
    // Calcular comissão (10% do total por padrão)
    const commissionRate = parseFloat(process.env.MECA_COMMISSION_RATE || "0.10")
    const total = Number(order.total || 0)
    const commissionAmount = Math.round(total * commissionRate)
    
    // Armazenar comissão nos metadata do pedido
    await orderModuleService.updateOrders(orderId, {
      metadata: {
        ...(order.metadata || {}),
        meca_commission_rate: commissionRate,
        meca_commission_amount: commissionAmount,
        meca_commission_calculated_at: new Date().toISOString(),
      }
    })
    
    logger.info(`Comissão calculada para Order ${orderId}: R$ ${(commissionAmount / 100).toFixed(2)} (${commissionRate * 100}%)`)
    
  } catch (error) {
    logger.error(`Erro ao calcular comissão para order ${data.id}:`, error)
  }
}

export const config: SubscriberConfig = {
  event: "order.placed",
}

