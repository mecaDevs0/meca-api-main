import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { SYSTEM_ALERT_MODULE } from "../../../modules/system_alert"

export const AUTHENTICATE = false

/**
 * GET /admin/alerts
 * 
 * Lista todos os alertas do sistema com filtros
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const {
    severity,
    category,
    is_read,
    is_resolved,
    limit = 50,
    offset = 0
  } = req.query
  
  const alertService = req.scope.resolve(SYSTEM_ALERT_MODULE)
  
  try {
    const filters: any = {}
    
    if (severity) filters.severity = severity
    if (category) filters.category = category
    if (is_read !== undefined) filters.is_read = is_read === 'true'
    if (is_resolved !== undefined) filters.is_resolved = is_resolved === 'true'
    
    const alerts = await alertService.listSystemAlerts(filters, {
      take: Number(limit),
      skip: Number(offset),
      order: { created_at: "DESC" }
    })
    
    // Estatísticas
    const stats = {
      total: alerts.length,
      unread: alerts.filter(a => !a.is_read).length,
      unresolved: alerts.filter(a => !a.is_resolved).length,
      critical: alerts.filter(a => a.severity === 'critical').length,
      high: alerts.filter(a => a.severity === 'high').length,
    }
    
    return res.json({
      alerts,
      stats,
      count: alerts.length,
      limit: Number(limit),
      offset: Number(offset)
    })
    
  } catch (error) {
    console.error("Erro ao listar alertas:", error)
    return res.status(500).json({
      message: "Erro ao listar alertas",
      error: error.message
    })
  }
}

/**
 * PUT /admin/alerts/:id/resolve
 * 
 * Marca um alerta como resolvido
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { id } = req.params
  const { resolved_by, notes } = req.body
  
  const alertService = req.scope.resolve(SYSTEM_ALERT_MODULE)
  
  try {
    const alert = await alertService.updateSystemAlerts(id, {
      is_resolved: true,
      is_read: true,
      resolved_at: new Date(),
      resolved_by,
      details: {
        ...alert.details,
        resolution_notes: notes
      }
    })
    
    return res.json({
      message: "Alerta resolvido",
      alert
    })
    
  } catch (error) {
    console.error("Erro ao resolver alerta:", error)
    return res.status(500).json({
      message: "Erro ao resolver alerta",
      error: error.message
    })
  }
}

