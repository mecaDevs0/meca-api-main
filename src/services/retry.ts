/**
 * Retry Service com Exponential Backoff
 * 
 * Sistema de retry automático para operações que podem falhar temporariamente
 * - Email sending
 * - API calls
 * - Database operations
 */

interface RetryOptions {
  maxAttempts?: number // Máximo de tentativas (padrão: 3)
  initialDelay?: number // Delay inicial em ms (padrão: 1000)
  maxDelay?: number // Delay máximo em ms (padrão: 30000)
  backoffMultiplier?: number // Multiplicador para exponential backoff (padrão: 2)
  onRetry?: (attempt: number, error: any) => void // Callback em cada retry
}

interface RetryResult<T> {
  success: boolean
  data?: T
  error?: any
  attempts: number
  totalTime: number
}

export class RetryService {
  /**
   * Executa uma função com retry automático
   */
  static async withRetry<T>(
    fn: () => Promise<T>,
    options: RetryOptions = {}
  ): Promise<RetryResult<T>> {
    const {
      maxAttempts = 3,
      initialDelay = 1000,
      maxDelay = 30000,
      backoffMultiplier = 2,
      onRetry
    } = options

    const startTime = Date.now()
    let lastError: any
    let delay = initialDelay

    for (let attempt = 1; attempt <= maxAttempts; attempt++) {
      try {
        const data = await fn()
        
        return {
          success: true,
          data,
          attempts: attempt,
          totalTime: Date.now() - startTime
        }
      } catch (error) {
        lastError = error
        
        if (attempt < maxAttempts) {
          // Chamar callback se fornecido
          if (onRetry) {
            onRetry(attempt, error)
          }
          
          // Log de retry
          console.log(`Retry attempt ${attempt}/${maxAttempts} failed. Retrying in ${delay}ms...`)
          
          // Aguardar antes de tentar novamente
          await this.sleep(delay)
          
          // Aumentar delay (exponential backoff)
          delay = Math.min(delay * backoffMultiplier, maxDelay)
        }
      }
    }

    // Todas as tentativas falharam
    return {
      success: false,
      error: lastError,
      attempts: maxAttempts,
      totalTime: Date.now() - startTime
    }
  }

  /**
   * Sleep helper
   */
  private static sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms))
  }

  /**
   * Retry com fallback
   * Tenta a operação primária, se falhar tenta o fallback
   */
  static async withFallback<T>(
    primaryFn: () => Promise<T>,
    fallbackFn: () => Promise<T>,
    options: RetryOptions = {}
  ): Promise<T> {
    const result = await this.withRetry(primaryFn, options)
    
    if (result.success && result.data !== undefined) {
      return result.data
    }
    
    // Tentar fallback
    console.warn('Primary function failed, using fallback')
    return await fallbackFn()
  }

  /**
   * Retry com circuit breaker
   * Para de tentar se houver muitas falhas consecutivas
   */
  private static circuitBreakers: Map<string, { failures: number; lastFailure: number; isOpen: boolean }> = new Map()
  
  static async withCircuitBreaker<T>(
    key: string,
    fn: () => Promise<T>,
    options: RetryOptions & { failureThreshold?: number; resetTimeout?: number } = {}
  ): Promise<T> {
    const {
      failureThreshold = 5,
      resetTimeout = 60000, // 1 minuto
      ...retryOptions
    } = options

    // Verificar estado do circuit breaker
    const breaker = this.circuitBreakers.get(key) || { failures: 0, lastFailure: 0, isOpen: false }

    // Se circuit breaker está aberto, verificar se já passou o timeout
    if (breaker.isOpen) {
      const timeSinceFailure = Date.now() - breaker.lastFailure
      
      if (timeSinceFailure < resetTimeout) {
        throw new Error(`Circuit breaker open for ${key}. Retry after ${Math.ceil((resetTimeout - timeSinceFailure) / 1000)}s`)
      }
      
      // Reset circuit breaker
      breaker.isOpen = false
      breaker.failures = 0
    }

    // Tentar executar
    const result = await this.withRetry(fn, retryOptions)

    if (result.success && result.data !== undefined) {
      // Sucesso - reset failures
      breaker.failures = 0
      this.circuitBreakers.set(key, breaker)
      return result.data
    }

    // Falha - incrementar contador
    breaker.failures++
    breaker.lastFailure = Date.now()

    if (breaker.failures >= failureThreshold) {
      breaker.isOpen = true
      console.error(`Circuit breaker opened for ${key} after ${breaker.failures} failures`)
    }

    this.circuitBreakers.set(key, breaker)
    throw result.error
  }

  /**
   * Retry específico para emails
   */
  static async retryEmail(
    emailFn: () => Promise<boolean>
  ): Promise<RetryResult<boolean>> {
    return this.withRetry(emailFn, {
      maxAttempts: 3,
      initialDelay: 2000,
      maxDelay: 10000,
      onRetry: (attempt, error) => {
        console.log(`Email retry ${attempt}: ${error.message}`)
      }
    })
  }

  /**
   * Retry específico para API calls
   */
  static async retryApiCall<T>(
    apiFn: () => Promise<T>
  ): Promise<T> {
    const result = await this.withRetry(apiFn, {
      maxAttempts: 3,
      initialDelay: 500,
      maxDelay: 5000
    })

    if (!result.success) {
      throw result.error
    }

    return result.data!
  }

  /**
   * Retry específico para Database operations
   */
  static async retryDbOperation<T>(
    dbFn: () => Promise<T>
  ): Promise<T> {
    const result = await this.withRetry(dbFn, {
      maxAttempts: 5,
      initialDelay: 100,
      maxDelay: 2000
    })

    if (!result.success) {
      throw result.error
    }

    return result.data!
  }
}

