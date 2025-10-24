import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { EntityManager } from "typeorm"

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const customerId = req.user?.customer_id

  if (!customerId) {
    res.status(401).json({ error: "Usuário não autenticado" })
    return
  }

  try {
    const { vehicleId } = req.query

    let query = `
      SELECT 
        mh.id,
        mh.vehicle_id,
        mh.workshop_name,
        mh.service_name,
        mh.service_description,
        mh.completion_date,
        mh.price_paid,
        mh.notes,
        mh.workshop_notes,
        mh.created_at,
        v.plate as vehicle_plate,
        v.brand as vehicle_brand,
        v.model as vehicle_model,
        v.year as vehicle_year
      FROM maintenance_history mh
      JOIN vehicles v ON mh.vehicle_id = v.id
      WHERE mh.customer_id = $1
    `

    const params = [customerId]

    if (vehicleId) {
      query += ` AND mh.vehicle_id = $2`
      params.push(vehicleId as string)
    }

    query += ` ORDER BY mh.completion_date DESC`

    const history = await manager.query(query, params)

    res.json({
      success: true,
      data: {
        maintenanceHistory: history,
        total: history.length
      }
    })

  } catch (error) {
    console.error("Erro ao buscar histórico de manutenção:", error)
    res.status(500).json({ 
      success: false, 
      error: "Erro interno do servidor" 
    })
  }
}
