import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * GET /store/workshops/me/services
 * 
 * Lista todos os serviços oferecidos pela oficina autenticada
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }
    
    const oficinaId = oficinas[0].id
    
    // Por enquanto, retornar todos os produtos
    // Quando o link oficina->product estiver funcionando, filtraremos por oficina
    const products = await productModuleService.listProducts({})
    
    return res.json({
      services: products,
      count: products.length,
    })
    
  } catch (error) {
    console.error("Erro ao listar serviços:", error)
    
    return res.status(500).json({
      message: "Erro ao listar serviços",
      error: error.message
    })
  }
}

/**
 * POST /store/workshops/me/services
 * 
 * Cria um novo serviço para a oficina autenticada
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const {
    title,
    description,
    price,
    duration_minutes,
    thumbnail,
    images
  } = req.body
  
  // Validações
  if (!title || !price) {
    return res.status(400).json({
      message: "Título e preço são obrigatórios"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }
    
    const oficinaId = oficinas[0].id
    
    // Criar produto (serviço)
    const product = await productModuleService.createProducts({
      title,
      description,
      is_giftcard: false,
      discountable: true,
      thumbnail,
      images: images || [],
      metadata: {
        oficina_id: oficinaId,
        duration_minutes: duration_minutes || null
      },
      // Criar uma variante padrão com o preço
      variants: [
        {
          title: "Padrão",
          prices: [
            {
              amount: price * 100, // Converter para centavos
              currency_code: "brl",
            }
          ],
          manage_inventory: false,
        }
      ]
    })
    
    return res.status(201).json({
      message: "Serviço criado com sucesso",
      service: product
    })
    
  } catch (error) {
    console.error("Erro ao criar serviço:", error)
    
    return res.status(500).json({
      message: "Erro ao criar serviço",
      error: error.message
    })
  }
}

