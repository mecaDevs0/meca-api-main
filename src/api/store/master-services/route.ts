import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { MASTER_SERVICE_MODULE } from "../../../modules/master_service"

/**
 * GET /store/master-services
 * 
 * Lista serviços master disponíveis (para oficinas selecionarem)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const masterServiceService = req.scope.resolve(MASTER_SERVICE_MODULE)
  
  const { category } = req.query
  
  const filters: any = {
    is_active: true
  }
  
  if (category) {
    filters.category = category
  }
  
  try {
    const services = await masterServiceService.listMasterServices(filters, {
      order: { display_order: "ASC", name: "ASC" }
    })
    
    return res.json({
      services,
      count: services.length,
    })
    
  } catch (error) {
    console.error("Erro ao listar serviços master:", error)
    
    return res.status(500).json({
      message: "Erro ao listar serviços",
      error: error.message
    })
  }
}

