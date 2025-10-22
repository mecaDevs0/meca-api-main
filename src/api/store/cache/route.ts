/**
 * Endpoint de cache para performance
 * GET /store/cache - Obter dados em cache
 * POST /store/cache - Invalidar cache
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

// Cache simples em memória (em produção usar Redis)
const memoryCache = new Map<string, { data: any, expires: number }>()

/**
 * GET /store/cache
 * Obter dados em cache
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const { key } = req.query

    if (!key) {
      return res.status(400).json({
        message: "Chave do cache é obrigatória"
      })
    }

    const cached = memoryCache.get(key as string)
    
    if (!cached) {
      return res.status(404).json({
        message: "Cache não encontrado"
      })
    }

    if (cached.expires < Date.now()) {
      memoryCache.delete(key as string)
      return res.status(404).json({
        message: "Cache expirado"
      })
    }

    return res.json({
      success: true,
      key,
      data: cached.data,
      expires_at: new Date(cached.expires).toISOString()
    })

  } catch (error) {
    console.error("Erro ao obter cache:", error)
    return res.status(500).json({
      message: "Erro ao obter cache",
      error: error.message
    })
  }
}

/**
 * POST /store/cache
 * Invalidar cache
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const { action, key, data, ttl } = req.body

    if (action === 'invalidate') {
      if (!key) {
        return res.status(400).json({
          message: "Chave do cache é obrigatória para invalidação"
        })
      }

      const deleted = memoryCache.delete(key)
      
      return res.json({
        success: true,
        action: 'invalidated',
        key,
        deleted
      })
    }

    if (action === 'set') {
      if (!key || !data) {
        return res.status(400).json({
          message: "Chave e dados são obrigatórios para definir cache"
        })
      }

      const expires = Date.now() + (ttl || 300000) // 5 minutos por padrão
      memoryCache.set(key, { data, expires })

      return res.json({
        success: true,
        action: 'set',
        key,
        expires_at: new Date(expires).toISOString()
      })
    }

    if (action === 'clear') {
      memoryCache.clear()
      
      return res.json({
        success: true,
        action: 'cleared',
        message: 'Todo o cache foi limpo'
      })
    }

    return res.status(400).json({
      message: "Ação inválida. Use: invalidate, set, clear"
    })

  } catch (error) {
    console.error("Erro ao gerenciar cache:", error)
    return res.status(500).json({
      message: "Erro ao gerenciar cache",
      error: error.message
    })
  }
}
