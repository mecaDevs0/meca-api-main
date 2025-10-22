/**
 * Cache Service com Redis + Fallback
 * 
 * Sistema de cache com múltiplas camadas:
 * - Layer 1: Redis (rápido, distribuído)
 * - Layer 2: In-memory (fallback se Redis falhar)
 * - Layer 3: Database (último recurso)
 */

interface CacheOptions {
  ttl?: number // Time to live em segundos
  tags?: string[] // Tags para invalidação em grupo
}

interface CacheStats {
  hits: number
  misses: number
  errors: number
  hitRate: number
}

export class CacheService {
  private static memoryCache: Map<string, { value: any; expiry: number; tags: string[] }> = new Map()
  private static stats: CacheStats = { hits: 0, misses: 0, errors: 0, hitRate: 0 }
  
  // TTL padrão por tipo de dado (em segundos)
  private static readonly DEFAULT_TTL = {
    workshops: 300, // 5 minutos
    customers: 600, // 10 minutos
    bookings: 60, // 1 minuto
    services: 1800, // 30 minutos
    reviews: 300, // 5 minutos
    default: 180 // 3 minutos
  }

  /**
   * Busca valor no cache (Redis > Memory > null)
   */
  static async get<T = any>(key: string): Promise<T | null> {
    try {
      // Tentar Redis primeiro
      try {
        // TODO: Implementar Redis quando disponível
        // const redisValue = await redis.get(key)
        // if (redisValue) {
        //   this.stats.hits++
        //   return JSON.parse(redisValue)
        // }
      } catch (redisError) {
        console.warn('Redis unavailable, using memory cache:', redisError.message)
      }

      // Fallback para memória
      const cached = this.memoryCache.get(key)
      
      if (cached && cached.expiry > Date.now()) {
        this.stats.hits++
        this.updateHitRate()
        return cached.value as T
      }

      // Cache expirado ou não existe
      if (cached) {
        this.memoryCache.delete(key)
      }

      this.stats.misses++
      this.updateHitRate()
      return null
    } catch (error) {
      this.stats.errors++
      console.error('Cache get error:', error)
      return null
    }
  }

  /**
   * Salva valor no cache (Redis + Memory)
   */
  static async set(key: string, value: any, options: CacheOptions = {}): Promise<void> {
    try {
      const ttl = options.ttl || this.DEFAULT_TTL.default
      const expiry = Date.now() + (ttl * 1000)
      const tags = options.tags || []

      // Salvar no Redis
      try {
        // TODO: Implementar Redis quando disponível
        // await redis.setex(key, ttl, JSON.stringify(value))
        // if (tags.length > 0) {
        //   for (const tag of tags) {
        //     await redis.sadd(`tag:${tag}`, key)
        //   }
        // }
      } catch (redisError) {
        console.warn('Redis unavailable:', redisError.message)
      }

      // Salvar em memória (fallback)
      this.memoryCache.set(key, { value, expiry, tags })
    } catch (error) {
      this.stats.errors++
      console.error('Cache set error:', error)
    }
  }

  /**
   * Remove valor do cache
   */
  static async delete(key: string): Promise<void> {
    try {
      // Remover do Redis
      try {
        // TODO: await redis.del(key)
      } catch (redisError) {
        console.warn('Redis unavailable:', redisError.message)
      }

      // Remover da memória
      this.memoryCache.delete(key)
    } catch (error) {
      console.error('Cache delete error:', error)
    }
  }

  /**
   * Invalida cache por tags
   */
  static async invalidateByTags(tags: string[]): Promise<void> {
    try {
      // Invalidar no Redis
      try {
        // TODO: Implementar com Redis
        // for (const tag of tags) {
        //   const keys = await redis.smembers(`tag:${tag}`)
        //   if (keys.length > 0) {
        //     await redis.del(...keys)
        //     await redis.del(`tag:${tag}`)
        //   }
        // }
      } catch (redisError) {
        console.warn('Redis unavailable:', redisError.message)
      }

      // Invalidar na memória
      for (const [key, cached] of this.memoryCache.entries()) {
        if (cached.tags.some(tag => tags.includes(tag))) {
          this.memoryCache.delete(key)
        }
      }
    } catch (error) {
      console.error('Cache invalidate error:', error)
    }
  }

  /**
   * Limpa todo o cache
   */
  static async flush(): Promise<void> {
    try {
      // Limpar Redis
      try {
        // TODO: await redis.flushall()
      } catch (redisError) {
        console.warn('Redis unavailable:', redisError.message)
      }

      // Limpar memória
      this.memoryCache.clear()
      console.log('Cache flushed')
    } catch (error) {
      console.error('Cache flush error:', error)
    }
  }

  /**
   * Retorna estatísticas do cache
   */
  static getStats(): CacheStats {
    return { ...this.stats }
  }

  /**
   * Atualiza hit rate
   */
  private static updateHitRate() {
    const total = this.stats.hits + this.stats.misses
    this.stats.hitRate = total > 0 ? this.stats.hits / total : 0
  }

  /**
   * Cache helper para queries
   */
  static async remember<T>(
    key: string,
    ttl: number,
    fetchFn: () => Promise<T>
  ): Promise<T> {
    // Tentar buscar do cache
    const cached = await this.get<T>(key)
    if (cached !== null) {
      return cached
    }

    // Buscar dados e cachear
    const data = await fetchFn()
    await this.set(key, data, { ttl })
    
    return data
  }

  /**
   * Limpa caches expirados (executar periodicamente)
   */
  static cleanExpired(): void {
    const now = Date.now()
    let cleaned = 0

    for (const [key, cached] of this.memoryCache.entries()) {
      if (cached.expiry <= now) {
        this.memoryCache.delete(key)
        cleaned++
      }
    }

    if (cleaned > 0) {
      console.log(`Cleaned ${cleaned} expired cache entries`)
    }
  }
}


