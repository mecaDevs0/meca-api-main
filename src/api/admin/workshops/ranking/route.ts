import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../modules/oficina"
import { BOOKING_MODULE } from "../../../../modules/booking"
import { REVIEW_MODULE } from "../../../../modules/review"

export const AUTHENTICATE = false

/**
 * GET /admin/workshops/ranking
 * 
 * Retorna ranking completo de oficinas com métricas de performance
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { period = '30', limit = 10 } = req.query
  
  const oficinaService = req.scope.resolve(OFICINA_MODULE)
  const bookingService = req.scope.resolve(BOOKING_MODULE)
  const reviewService = req.scope.resolve(REVIEW_MODULE)
  
  try {
    // Buscar todas oficinas aprovadas
    const oficinas = await oficinaService.listOficinas({
      status: "aprovado"
    })
    
    // Data de referência para período
    const periodStart = new Date()
    periodStart.setDate(periodStart.getDate() - parseInt(period as string))
    
    // Calcular métricas para cada oficina
    const rankingData = await Promise.all(
      oficinas.map(async (oficina) => {
        // Total de agendamentos
        const bookings = await bookingService.listBookings({
          oficina_id: oficina.id,
          created_at: { $gte: periodStart.toISOString() }
        })
        
        const totalBookings = bookings.length
        const confirmedBookings = bookings.filter(b => 
          ['confirmado', 'finalizado_mecanico', 'finalizado_cliente'].includes(b.status)
        ).length
        
        const acceptanceRate = totalBookings > 0 
          ? Math.round((confirmedBookings / totalBookings) * 100) 
          : 0
        
        // Avaliações
        const reviews = await reviewService.listReviews({
          oficina_id: oficina.id
        })
        
        const avgRating = reviews.length > 0
          ? reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length
          : 0
        
        // Receita total
        const revenue = bookings.reduce((sum, b) => {
          return sum + (b.final_price || b.estimated_price || 0)
        }, 0)
        
        // Taxa de conclusão
        const completedBookings = bookings.filter(b => 
          b.status === 'finalizado_cliente'
        ).length
        
        const completionRate = totalBookings > 0
          ? Math.round((completedBookings / totalBookings) * 100)
          : 0
        
        // Tempo médio de resposta (simples - calcular depois)
        const avgResponseTime = Math.random() * 24 // Mock por enquanto - implementar depois
        
        return {
          oficina_id: oficina.id,
          name: oficina.name,
          logo_url: oficina.logo_url,
          address: oficina.address,
          
          // Métricas
          total_bookings: totalBookings,
          confirmed_bookings: confirmedBookings,
          completed_bookings: completedBookings,
          acceptance_rate: acceptanceRate,
          completion_rate: completionRate,
          
          // Avaliações
          total_reviews: reviews.length,
          average_rating: Math.round(avgRating * 10) / 10,
          
          // Financeiro
          total_revenue: revenue,
          avg_ticket: totalBookings > 0 ? Math.round(revenue / totalBookings) : 0,
          
          // Performance
          avg_response_time_hours: Math.round(avgResponseTime * 10) / 10,
          
          // Score geral (ponderado)
          performance_score: calculatePerformanceScore({
            avgRating,
            acceptanceRate,
            completionRate,
            totalBookings
          })
        }
      })
    )
    
    // Ordenar por score de performance
    rankingData.sort((a, b) => b.performance_score - a.performance_score)
    
    // Adicionar posição
    const ranking = rankingData.slice(0, parseInt(limit as string)).map((item, index) => ({
      position: index + 1,
      medal: index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : '',
      ...item
    }))
    
    // Insights automáticos
    const insights = generateInsights(ranking)
    
    return res.json({
      period_days: parseInt(period as string),
      total_workshops: oficinas.length,
      ranking,
      insights
    })
    
  } catch (error) {
    console.error("Erro ao gerar ranking:", error)
    return res.status(500).json({
      message: "Erro ao gerar ranking",
      error: error.message
    })
  }
}

function calculatePerformanceScore(metrics: any): number {
  const {
    avgRating,
    acceptanceRate,
    completionRate,
    totalBookings
  } = metrics
  
  // Pesos
  const ratingWeight = 0.35
  const acceptanceWeight = 0.25
  const completionWeight = 0.25
  const volumeWeight = 0.15
  
  // Normalizar volume (assumindo max de 1000 agendamentos/mês)
  const normalizedVolume = Math.min(totalBookings / 1000, 1) * 100
  
  const score = 
    (avgRating * 20 * ratingWeight) + // Rating de 0-5 -> 0-100
    (acceptanceRate * acceptanceWeight) +
    (completionRate * completionWeight) +
    (normalizedVolume * volumeWeight)
  
  return Math.round(score * 10) / 10
}

function generateInsights(ranking: any[]): any[] {
  const insights = []
  
  if (ranking.length === 0) return insights
  
  // Top performer
  const top = ranking[0]
  if (top.acceptance_rate >= 95) {
    insights.push({
      type: "success",
      icon: "💡",
      message: `${top.name} tem ${top.acceptance_rate}% de taxa de aceitação - Considere destacar como "Parceiro Premium"`
    })
  }
  
  // Oficinas com baixo desempenho
  const lowPerformers = ranking.filter(r => r.average_rating < 4.0)
  if (lowPerformers.length > 0) {
    insights.push({
      type: "warning",
      icon: "⚠️",
      message: `${lowPerformers.length} oficina(s) com avaliação abaixo de 4.0 - Envie feedback e suporte`
    })
  }
  
  // Oficinas em crescimento
  const highVolume = ranking.filter(r => r.total_bookings > 100)
  if (highVolume.length > 0) {
    insights.push({
      type: "info",
      icon: "📈",
      message: `${highVolume.length} oficina(s) com mais de 100 agendamentos - Alto volume`
    })
  }
  
  return insights
}

