import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../modules/oficina"
import { OficinaStatus } from "../../../modules/oficina/models/oficina"

/**
 * GET /store/services
 * 
 * Busca serviços disponíveis com filtros opcionais
 * 
 * Query params:
 * - q: termo de busca (nome do serviço)
 * - cidade: filtro por cidade
 * - estado: filtro por estado
 * - limit: limite de resultados (default: 20)
 * - offset: offset para paginação (default: 0)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const query = req.scope.resolve("query")
  
  const { 
    q, 
    cidade, 
    estado, 
    limit = 20, 
    offset = 0 
  } = req.query
  
  // Buscar apenas oficinas aprovadas
  const oficinasAprovadas = await oficinaModuleService.listOficinas({
    status: OficinaStatus.APROVADO
  })
  
  const oficinaIds = oficinasAprovadas.map(o => o.id)
  
  // Se não houver oficinas aprovadas, retornar vazio
  if (oficinaIds.length === 0) {
    return res.json({
      services: [],
      count: 0,
      limit: Number(limit),
      offset: Number(offset)
    })
  }
  
  // Construir filtros de busca
  const filters: any = {}
  
  if (q) {
    filters.title = {
      $ilike: `%${q}%`
    }
  }
  
  // Buscar produtos/serviços
  const { data: products, metadata } = await productModuleService.listAndCountProducts(
    filters,
    {
      take: Number(limit),
      skip: Number(offset),
      relations: ["variants"]
    }
  )
  
  // Enriquecer com dados da oficina usando o serviço de query
  const servicesWithOficina = await Promise.all(
    products.map(async (product) => {
      // Buscar oficina vinculada ao produto
      const { data } = await query.graph({
        entity: "product",
        fields: ["id", "title", "description", "thumbnail", "oficina.*"],
        filters: { id: product.id }
      })
      
      return {
        ...product,
        oficina: data[0]?.oficina || null
      }
    })
  )
  
  // Filtrar por localização se especificado
  let filteredServices = servicesWithOficina
  
  if (cidade || estado) {
    filteredServices = servicesWithOficina.filter(service => {
      const oficina = service.oficina
      if (!oficina?.address) return false
      
      const matchCidade = cidade ? oficina.address.cidade?.toLowerCase() === cidade.toLowerCase() : true
      const matchEstado = estado ? oficina.address.estado?.toLowerCase() === estado.toLowerCase() : true
      
      return matchCidade && matchEstado
    })
  }
  
  return res.json({
    services: filteredServices,
    count: metadata.count,
    limit: Number(limit),
    offset: Number(offset)
  })
}

