import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../../../modules/oficina"
import { BOOKING_MODULE } from "../../../../../modules/booking"

/**
 * GET /store/workshops/me/financial?start_date=&end_date=
 * 
 * Histórico financeiro da oficina
 * Retorna todos os serviços pagos com valores e comissões
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const orderModuleService = req.scope.resolve(Modules.ORDER)
  
  const userId = req.auth_context?.actor_id
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const { start_date, end_date } = req.query
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    
    // Buscar bookings finalizados e pagos
    const filters: any = {
      oficina_id: oficinaId,
      status: "finalizado_cliente"
    }
    
    if (start_date) {
      filters.completed_at = { $gte: new Date(start_date as string) }
    }
    
    if (end_date) {
      filters.completed_at = { 
        ...(filters.completed_at || {}),
        $lte: new Date(end_date as string)
      }
    }
    
    const bookings = await bookingModuleService.listBookings(filters, {
      order: { completed_at: "DESC" }
    })
    
    // Buscar orders associados e calcular totais
    const transactions = await Promise.all(
      bookings.map(async (booking) => {
        let order = null
        let commission = 0
        let netAmount = booking.final_price || booking.estimated_price || 0
        
        if (booking.order_id) {
          try {
            order = await orderModuleService.retrieveOrder(booking.order_id)
            commission = order.metadata?.meca_commission_amount || 0
            netAmount = (booking.final_price || booking.estimated_price || 0) - commission
          } catch (e) {
            // Order não encontrado
          }
        }
        
        return {
          booking_id: booking.id,
          order_id: booking.order_id,
          appointment_date: booking.appointment_date,
          completed_at: booking.completed_at,
          gross_amount: booking.final_price || booking.estimated_price,
          commission_amount: commission,
          net_amount: netAmount,
          vehicle: booking.vehicle_snapshot,
          customer_notes: booking.customer_notes,
        }
      })
    )
    
    // Calcular totais
    const totalGross = transactions.reduce((sum, t) => sum + (t.gross_amount || 0), 0)
    const totalCommission = transactions.reduce((sum, t) => sum + t.commission_amount, 0)
    const totalNet = transactions.reduce((sum, t) => sum + t.net_amount, 0)
    
    return res.json({
      transactions,
      summary: {
        total_transactions: transactions.length,
        total_gross: totalGross,
        total_commission: totalCommission,
        total_net: totalNet,
        period: {
          start: start_date || null,
          end: end_date || null,
        }
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar histórico financeiro:", error)
    
    return res.status(500).json({
      message: "Erro ao buscar histórico financeiro",
      error: error.message
    })
  }
}

