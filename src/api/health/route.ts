import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { EmailService } from "../../services/email"

export const AUTHENTICATE = false

interface HealthCheckResult {
  status: 'healthy' | 'degraded' | 'unhealthy'
  timestamp: string
  uptime: number
  checks: {
    database: HealthCheckDetail
    redis: HealthCheckDetail
    email: HealthCheckDetail
  }
  version: string
  environment: string
}

interface HealthCheckDetail {
  status: 'pass' | 'fail' | 'warn'
  responseTime?: number
  message?: string
  details?: any
}

/**
 * GET /health
 * 
 * Endpoint completo de health check para monitoramento
 * Verifica: Database, Redis, Email SMTP
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const startTime = Date.now()
  
  // Parallel health checks para performance
  const [dbCheck, redisCheck, emailCheck] = await Promise.allSettled([
    checkDatabase(req),
    checkRedis(req),
    checkEmail()
  ])

  const result: HealthCheckResult = {
    status: 'healthy',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    checks: {
      database: dbCheck.status === 'fulfilled' ? dbCheck.value : { status: 'fail', message: 'Database check failed' },
      redis: redisCheck.status === 'fulfilled' ? redisCheck.value : { status: 'fail', message: 'Redis check failed' },
      email: emailCheck.status === 'fulfilled' ? emailCheck.value : { status: 'fail', message: 'Email check failed' }
    },
    version: process.env.npm_package_version || '1.0.0',
    environment: process.env.NODE_ENV || 'development'
  }

  // Determinar status geral
  const allChecks = Object.values(result.checks)
  const hasFailures = allChecks.some(check => check.status === 'fail')
  const hasWarnings = allChecks.some(check => check.status === 'warn')

  if (hasFailures) {
    result.status = 'unhealthy'
  } else if (hasWarnings) {
    result.status = 'degraded'
  }

  const statusCode = result.status === 'healthy' ? 200 : result.status === 'degraded' ? 200 : 503

  return res.status(statusCode).json(result)
}

/**
 * Verifica conexão com o banco de dados PostgreSQL
 */
async function checkDatabase(req: MedusaRequest): Promise<HealthCheckDetail> {
  const startTime = Date.now()
  
  try {
    // Tenta fazer uma query simples
    const customerService = req.scope.resolve(Modules.CUSTOMER)
    await customerService.listCustomers({ take: 1 })
    
    const responseTime = Date.now() - startTime
    
    return {
      status: responseTime < 1000 ? 'pass' : 'warn',
      responseTime,
      message: responseTime < 1000 ? 'Database connection healthy' : 'Database slow response',
      details: {
        type: 'PostgreSQL',
        url: process.env.DATABASE_URL?.split('@')[1]?.split('?')[0] || 'unknown'
      }
    }
  } catch (error) {
    return {
      status: 'fail',
      responseTime: Date.now() - startTime,
      message: 'Database connection failed',
      details: {
        error: error.message
      }
    }
  }
}

/**
 * Verifica conexão com Redis
 */
async function checkRedis(req: MedusaRequest): Promise<HealthCheckDetail> {
  const startTime = Date.now()
  
  try {
    // Tenta resolver o cache manager
    const cacheService = req.scope.resolve('cacheService')
    
    // Se existe, verifica se está funcionando
    if (cacheService) {
      // Tenta fazer um set/get simples
      await cacheService.set('health:check', '1', 10)
      const value = await cacheService.get('health:check')
      
      const responseTime = Date.now() - startTime
      
      return {
        status: value === '1' ? 'pass' : 'warn',
        responseTime,
        message: 'Redis connection healthy',
        details: {
          url: process.env.REDIS_URL || 'localhost:6379'
        }
      }
    }
    
    // Redis não configurado mas não é crítico
    return {
      status: 'warn',
      responseTime: Date.now() - startTime,
      message: 'Redis not configured (using in-memory cache)',
      details: {
        note: 'Redis is optional, application running with fallback'
      }
    }
  } catch (error) {
    // Redis falhou mas temos fallback
    return {
      status: 'warn',
      responseTime: Date.now() - startTime,
      message: 'Redis unavailable, using fallback',
      details: {
        error: error.message,
        fallback: 'in-memory'
      }
    }
  }
}

/**
 * Verifica conexão SMTP para envio de emails
 */
async function checkEmail(): Promise<HealthCheckDetail> {
  const startTime = Date.now()
  
  try {
    const isConnected = await EmailService.verifyConnection()
    const responseTime = Date.now() - startTime
    
    if (isConnected) {
      return {
        status: 'pass',
        responseTime,
        message: 'SMTP connection healthy',
        details: {
          host: process.env.SMTP_HOST || 'smtp.gmail.com',
          port: process.env.SMTP_PORT || 587,
          from: process.env.SMTP_EMAIL || 'unknown'
        }
      }
    }
    
    return {
      status: 'fail',
      responseTime,
      message: 'SMTP connection failed',
      details: {
        host: process.env.SMTP_HOST || 'smtp.gmail.com'
      }
    }
  } catch (error) {
    return {
      status: 'fail',
      responseTime: Date.now() - startTime,
      message: 'Email service check failed',
      details: {
        error: error.message
      }
    }
  }
}

/**
 * GET /health/ready
 * 
 * Readiness probe - verifica se a aplicação está pronta para receber requisições
 */
export async function ready(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    // Verifica se o banco está acessível
    const customerService = req.scope.resolve(Modules.CUSTOMER)
    await customerService.listCustomers({ take: 1 })
    
    return res.json({
      status: 'ready',
      timestamp: new Date().toISOString()
    })
  } catch (error) {
    return res.status(503).json({
      status: 'not ready',
      timestamp: new Date().toISOString(),
      error: error.message
    })
  }
}

/**
 * GET /health/live
 * 
 * Liveness probe - verifica se a aplicação está viva
 */
export async function live(
  req: MedusaRequest,
  res: MedusaResponse
) {
  return res.json({
    status: 'alive',
    timestamp: new Date().toISOString(),
    uptime: process.uptime(),
    memory: process.memoryUsage(),
    pid: process.pid
  })
}

