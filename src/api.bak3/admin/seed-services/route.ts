import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { MASTER_SERVICE_MODULE } from "../../../modules/master_service"

export const AUTHENTICATE = false

/**
 * POST /admin/seed-services
 * Endpoint especial para popular serviços iniciais (SEM AUTH para seed)
 */
export async function POST(req: MedusaRequest, res: MedusaResponse) {
  const masterServiceService = req.scope.resolve(MASTER_SERVICE_MODULE)
  
  const services = [
    {
      name: "Troca de Óleo",
      description: "Troca de óleo do motor e filtro",
      category: "Manutenção Preventiva",
      estimated_duration_minutes: 30,
      suggested_price_min: 10000,
      suggested_price_max: 25000,
      display_order: 1,
      is_active: true
    },
    {
      name: "Alinhamento e Balanceamento",
      description: "Alinhamento e balanceamento de rodas",
      category: "Manutenção Preventiva",
      estimated_duration_minutes: 60,
      suggested_price_min: 15000,
      suggested_price_max: 30000,
      display_order: 2,
      is_active: true
    },
    {
      name: "Troca de Pneus",
      description: "Troca de pneus com montagem e balanceamento",
      category: "Pneus e Rodas",
      estimated_duration_minutes: 90,
      suggested_price_min: 50000,
      suggested_price_max: 200000,
      display_order: 3,
      is_active: true
    },
    {
      name: "Revisão Geral",
      description: "Revisão completa do veículo conforme manual",
      category: "Manutenção Preventiva",
      estimated_duration_minutes: 120,
      suggested_price_min: 30000,
      suggested_price_max: 80000,
      display_order: 4,
      is_active: true
    },
    {
      name: "Freios",
      description: "Troca de pastilhas e discos de freio",
      category: "Sistema de Freios",
      estimated_duration_minutes: 90,
      suggested_price_min: 40000,
      suggested_price_max: 150000,
      display_order: 5,
      is_active: true
    }
  ]
  
  try {
    const created = []
    
    for (const service of services) {
      const result = await masterServiceService.createMasterServices(service)
      created.push(result)
    }
    
    return res.status(201).json({
      message: "Serviços base criados com sucesso",
      services: created
    })
  } catch (error) {
    return res.status(500).json({
      message: "Erro ao criar serviços",
      error: error.message
    })
  }
}

