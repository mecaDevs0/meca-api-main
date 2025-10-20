import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { VEHICLE_HISTORY_MODULE } from "../../../../../modules/vehicle_history"

export const AUTHENTICATE = false

/**
 * GET /store/vehicles/:id/history
 * 
 * Retorna histórico completo de manutenção de um veículo
 * Timeline ordenada cronologicamente
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { id } = req.params
  const vehicleHistoryService = req.scope.resolve(VEHICLE_HISTORY_MODULE)
  
  try {
    const history = await vehicleHistoryService.listVehicleHistories(
      { vehicle_id: id },
      {
        select: [
          "id",
          "service_name",
          "workshop_name",
          "service_date",
          "odometer_reading",
          "next_service_km",
          "next_service_date",
          "notes",
          "parts_replaced",
          "parts_cost",
          "labor_cost",
          "total_cost",
          "warranty_months",
          "warranty_expires_at"
        ],
        order: { service_date: "DESC" }
      }
    )
    
    // Calcular próximas manutenções recomendadas
    const recommendations = calculateMaintenanceRecommendations(history)
    
    return res.json({
      vehicle_id: id,
      total_services: history.length,
      history,
      recommendations,
      last_service: history[0] || null
    })
    
  } catch (error) {
    console.error("Erro ao buscar histórico:", error)
    return res.status(500).json({
      message: "Erro ao buscar histórico",
      error: error.message
    })
  }
}

function calculateMaintenanceRecommendations(history: any[]) {
  if (!history || history.length === 0) {
    return [
      {
        type: "oil_change",
        urgency: "medium",
        message: "Recomendamos fazer a primeira revisão"
      }
    ]
  }
  
  const recommendations = []
  const lastService = history[0]
  const now = new Date()
  
  // Verificar garantias expirando
  history.forEach(service => {
    if (service.warranty_expires_at) {
      const expiryDate = new Date(service.warranty_expires_at)
      const daysUntilExpiry = Math.floor((expiryDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
      
      if (daysUntilExpiry > 0 && daysUntilExpiry <= 30) {
        recommendations.push({
          type: "warranty_expiring",
          urgency: "high",
          message: `Garantia de "${service.service_name}" expira em ${daysUntilExpiry} dias`,
          service_id: service.id
        })
      }
    }
  })
  
  // Verificar manutenção periódica
  if (lastService.next_service_date) {
    const nextDate = new Date(lastService.next_service_date)
    const daysUntilNext = Math.floor((nextDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
    
    if (daysUntilNext <= 7) {
      recommendations.push({
        type: "service_due",
        urgency: "high",
        message: `Próxima manutenção recomendada em ${daysUntilNext} dias`,
        due_date: lastService.next_service_date
      })
    }
  }
  
  return recommendations
}

