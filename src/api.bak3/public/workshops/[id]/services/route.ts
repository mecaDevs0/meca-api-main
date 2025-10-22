import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * GET /public/workshops/:id/services
 * 
 * Lista serviços oferecidos pela oficina
 */
export async function GET(
  req: MedusaRequest<{ id: string }>,
  res: MedusaResponse
) {
  const productService = req.scope.resolve(Modules.PRODUCT)
  const oficina_id = req.params.id

  try {
    // Buscar produtos (serviços) da oficina
    const products = await productService.listProducts({
      metadata: {
        oficina_id
      }
    })

    return res.json({
      services: products,
      count: products.length
    })

  } catch (error) {
    console.error("Erro ao listar serviços da oficina:", error)

    return res.status(500).json({
      message: "Erro ao listar serviços",
      error: error.message
    })
  }
}

/**
 * POST /public/workshops/:id/services
 * 
 * Adicionar serviço à oficina
 */
export async function POST(
  req: MedusaRequest<{ id: string }>,
  res: MedusaResponse
) {
  const productService = req.scope.resolve(Modules.PRODUCT)
  const oficina_id = req.params.id

  const {
    title,
    description,
    price,
    duration,
    master_service_id
  } = req.body

  if (!title || !price) {
    return res.status(400).json({
      message: "Título e preço são obrigatórios"
    })
  }

  try {
    // Criar produto (serviço)
    const product = await productService.createProducts({
      title,
      description,
      is_giftcard: false,
      discountable: true,
      options: [],
      variants: [
        {
          title: "Default",
          prices: [
            {
              amount: parseFloat(price) * 100, // Converter para centavos
              currency_code: "BRL"
            }
          ]
        }
      ],
      metadata: {
        oficina_id,
        master_service_id,
        duration
      }
    })

    return res.status(201).json({
      message: "Serviço adicionado com sucesso!",
      service: product
    })

  } catch (error) {
    console.error("Erro ao adicionar serviço:", error)

    return res.status(500).json({
      message: "Erro ao adicionar serviço",
      error: error.message
    })
  }
}

