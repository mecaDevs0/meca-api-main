import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * GET /store/services
 *
 * Listar todos os serviços disponíveis
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  
  try {
    const products = await productModuleService.listProducts({
      status: "published"
    })

    const formattedServices = products.map(product => ({
      id: product.id,
      title: product.title,
      description: product.description,
      price: product.price,
      images: product.images,
      metadata: product.metadata,
      created_at: product.created_at,
      updated_at: product.updated_at
    }))

    return res.json({
      products: formattedServices,
      total: formattedServices.length
    })

  } catch (error) {
    console.error("Erro ao buscar serviços:", error)
    return res.status(500).json({
      message: "Erro ao buscar serviços",
      error: error.message
    })
  }
}
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


