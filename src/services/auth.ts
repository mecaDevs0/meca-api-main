/**
 * Authentication Service
 * Handles password hashing and JWT token generation
 */

import bcrypt from 'bcrypt'
import jwt from 'jsonwebtoken'

const JWT_SECRET = process.env.JWT_SECRET || 'supersecret-change-in-production'
const JWT_EXPIRES_IN = '7d'
const SALT_ROUNDS = 10

// Cache para tokens válidos (opcional, para performance)
const tokenCache = new Map<string, { payload: any, expires: number }>()

export class AuthService {
  /**
   * Hash a password using bcrypt
   */
  static async hashPassword(password: string): Promise<string> {
    return await bcrypt.hash(password, SALT_ROUNDS)
  }

  /**
   * Compare a password with a hash
   */
  static async comparePassword(password: string, hash: string): Promise<boolean> {
    return await bcrypt.compare(password, hash)
  }

  /**
   * Generate a JWT token
   */
  static generateToken(payload: { id: string; email: string; type: 'workshop' | 'customer' }): string {
    return jwt.sign(payload, JWT_SECRET, { expiresIn: JWT_EXPIRES_IN })
  }

  /**
   * Verify a JWT token (otimizado com cache)
   */
  static verifyToken(token: string): any {
    // Verificar cache primeiro (opcional)
    const cached = tokenCache.get(token)
    if (cached && cached.expires > Date.now()) {
      return cached.payload
    }

    try {
      const decoded = jwt.verify(token, JWT_SECRET)
      
      // Cachear token válido por 5 minutos
      if (typeof decoded === 'object' && decoded !== null) {
        tokenCache.set(token, {
          payload: decoded,
          expires: Date.now() + 5 * 60 * 1000 // 5 minutos
        })
      }
      
      return decoded
    } catch (error) {
      // Limpar cache se token inválido
      tokenCache.delete(token)
      throw new Error('Invalid or expired token')
    }
  }

  /**
   * Extract token from Authorization header
   */
  static extractTokenFromHeader(authHeader: string | undefined): string | null {
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return null
    }
    return authHeader.substring(7)
  }

  /**
   * Limpar cache expirado (para performance)
   */
  static clearExpiredCache(): void {
    const now = Date.now()
    for (const [token, data] of tokenCache.entries()) {
      if (data.expires <= now) {
        tokenCache.delete(token)
      }
    }
  }

  /**
   * Invalidar token específico
   */
  static invalidateToken(token: string): void {
    tokenCache.delete(token)
  }
}


      
      return decoded
    } catch (error) {
      // Limpar cache se token inválido
      tokenCache.delete(token)
      throw new Error('Invalid or expired token')
    }
  }

  /**
   * Extract token from Authorization header
   */
  static extractTokenFromHeader(authHeader: string | undefined): string | null {
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return null
    }
    return authHeader.substring(7)
  }

  /**
   * Limpar cache expirado (para performance)
   */
  static clearExpiredCache(): void {
    const now = Date.now()
    for (const [token, data] of tokenCache.entries()) {
      if (data.expires <= now) {
        tokenCache.delete(token)
      }
    }
  }

  /**
   * Invalidar token específico
   */
  static invalidateToken(token: string): void {
    tokenCache.delete(token)
  }
}

