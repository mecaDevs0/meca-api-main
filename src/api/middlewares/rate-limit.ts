import { MedusaRequest, MedusaResponse, MedusaNextFunction } from "@medusajs/medusa"

/**
 * Rate Limiting Middleware
 * 
 * Protege API contra abuso e DDoS
 * - Limita requisições por IP
 * - Limita requisições por usuário autenticado
 * - Diferentes limites por tipo de endpoint
 */

interface RateLimitConfig {
  windowMs: number // Janela de tempo em ms
  maxRequests: number // Máximo de requests na janela
  message?: string // Mensagem customizada
}

interface RateLimitStore {
  [key: string]: {
    count: number
    resetTime: number
  }
}

export class RateLimiter {
  private static stores: Map<string, RateLimitStore> = new Map()

  /**
   * Cria middleware de rate limiting
   */
  static create(config: RateLimitConfig) {
    const storeName = `${config.windowMs}-${config.maxRequests}`
    
    if (!this.stores.has(storeName)) {
      this.stores.set(storeName, {})
    }

    return async (req: MedusaRequest, res: MedusaResponse, next: MedusaNextFunction) => {
      const store = this.stores.get(storeName)!
      
      // Identificar cliente (IP ou user ID)
      const identifier = req.auth_context?.actor_id || this.getClientIp(req)
      const now = Date.now()

      // Inicializar ou resetar contador
      if (!store[identifier] || store[identifier].resetTime <= now) {
        store[identifier] = {
          count: 0,
          resetTime: now + config.windowMs
        }
      }

      // Incrementar contador
      store[identifier].count++

      // Verificar limite
      if (store[identifier].count > config.maxRequests) {
        const retryAfter = Math.ceil((store[identifier].resetTime - now) / 1000)
        
        res.setHeader('X-RateLimit-Limit', config.maxRequests.toString())
        res.setHeader('X-RateLimit-Remaining', '0')
        res.setHeader('X-RateLimit-Reset', store[identifier].resetTime.toString())
        res.setHeader('Retry-After', retryAfter.toString())

        return res.status(429).json({
          message: config.message || 'Too many requests, please try again later',
          retryAfter: retryAfter
        })
      }

      // Adicionar headers informativos
      res.setHeader('X-RateLimit-Limit', config.maxRequests.toString())
      res.setHeader('X-RateLimit-Remaining', (config.maxRequests - store[identifier].count).toString())
      res.setHeader('X-RateLimit-Reset', store[identifier].resetTime.toString())

      next()
    }
  }

  /**
   * Rate limiter para autenticação (mais restritivo)
   */
  static forAuth() {
    return this.create({
      windowMs: 15 * 60 * 1000, // 15 minutos
      maxRequests: 5, // 5 tentativas
      message: 'Muitas tentativas de login. Tente novamente em alguns minutos.'
    })
  }

  /**
   * Rate limiter para API pública (moderado)
   */
  static forPublic() {
    return this.create({
      windowMs: 1 * 60 * 1000, // 1 minuto
      maxRequests: 60, // 60 requests/min
      message: 'Muitas requisições. Por favor, aguarde um momento.'
    })
  }

  /**
   * Rate limiter para usuários autenticados (menos restritivo)
   */
  static forAuthenticated() {
    return this.create({
      windowMs: 1 * 60 * 1000, // 1 minuto
      maxRequests: 120, // 120 requests/min
      message: 'Limite de requisições excedido. Aguarde um momento.'
    })
  }

  /**
   * Rate limiter para operações críticas (muito restritivo)
   */
  static forCritical() {
    return this.create({
      windowMs: 60 * 60 * 1000, // 1 hora
      maxRequests: 10, // 10 tentativas/hora
      message: 'Limite de operações críticas excedido. Tente novamente mais tarde.'
    })
  }

  /**
   * Extrai IP do cliente
   */
  private static getClientIp(req: MedusaRequest): string {
    const forwarded = req.headers['x-forwarded-for'] as string
    
    if (forwarded) {
      return forwarded.split(',')[0].trim()
    }
    
    return req.socket?.remoteAddress || 'unknown'
  }

  /**
   * Limpa entradas expiradas (executar periodicamente)
   */
  static cleanExpired(): void {
    const now = Date.now()
    let cleaned = 0

    for (const store of this.stores.values()) {
      for (const [key, entry] of Object.entries(store)) {
        if (entry.resetTime <= now) {
          delete store[key]
          cleaned++
        }
      }
    }

    if (cleaned > 0) {
      console.log(`Cleaned ${cleaned} expired rate limit entries`)
    }
  }
}

