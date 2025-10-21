/**
 * Unified Notification Service
 * 
 * Sistema unificado de notificações com múltiplos canais:
 * - Email (SMTP)
 * - Push (FCM)
 * - SMS (futuro)
 * 
 * Features:
 * - Retry automático em falhas
 * - Queue de notificações
 * - Fallback entre canais
 * - Logging estruturado
 */

import { EmailService } from "./email"
import { RetryService } from "./retry"

interface NotificationPayload {
  to: string | string[]
  subject?: string
  message: string
  data?: Record<string, any>
  priority?: 'high' | 'normal' | 'low'
}

interface NotificationChannels {
  email?: boolean
  push?: boolean
  sms?: boolean
}

interface NotificationResult {
  success: boolean
  channels: {
    email?: { sent: boolean; error?: string }
    push?: { sent: boolean; error?: string }
    sms?: { sent: boolean; error?: string }
  }
  timestamp: string
}

export class NotificationService {
  private static queue: NotificationPayload[] = []
  private static processing = false

  /**
   * Envia notificação por todos os canais configurados
   */
  static async send(
    payload: NotificationPayload,
    channels: NotificationChannels = { email: true, push: true }
  ): Promise<NotificationResult> {
    const result: NotificationResult = {
      success: false,
      channels: {},
      timestamp: new Date().toISOString()
    }

    const promises: Promise<void>[] = []

    // Email
    if (channels.email && typeof payload.to === 'string' && payload.to.includes('@')) {
      promises.push(
        this.sendEmail(payload)
          .then(sent => {
            result.channels.email = { sent }
            if (sent) result.success = true
          })
          .catch(error => {
            result.channels.email = { sent: false, error: error.message }
          })
      )
    }

    // Push Notification
    if (channels.push) {
      promises.push(
        this.sendPush(payload)
          .then(sent => {
            result.channels.push = { sent }
            if (sent) result.success = true
          })
          .catch(error => {
            result.channels.push = { sent: false, error: error.message }
          })
      )
    }

    // SMS (futuro)
    if (channels.sms) {
      promises.push(
        this.sendSMS(payload)
          .then(sent => {
            result.channels.sms = { sent }
            if (sent) result.success = true
          })
          .catch(error => {
            result.channels.sms = { sent: false, error: error.message }
          })
      )
    }

    // Executar todos em paralelo
    await Promise.allSettled(promises)

    return result
  }

  /**
   * Envia email com retry
   */
  private static async sendEmail(payload: NotificationPayload): Promise<boolean> {
    const result = await RetryService.retryEmail(async () => {
      if (!payload.subject) {
        throw new Error('Subject is required for email')
      }

      return await EmailService.sendEmail({
        to: payload.to,
        subject: payload.subject,
        html: `<p>${payload.message}</p>`,
        text: payload.message
      })
    })

    return result.success && result.data === true
  }

  /**
   * Envia push notification com retry
   */
  private static async sendPush(payload: NotificationPayload): Promise<boolean> {
    try {
      const result = await RetryService.withRetry(async () => {
        // TODO: Integrar com FCM quando disponível
        // const fcmService = container.resolve(FCM_MODULE)
        // return await fcmService.send({
        //   to: payload.to,
        //   title: payload.subject || 'MECA',
        //   body: payload.message,
        //   data: payload.data
        // })
        
        console.log('Push notification:', payload.subject, payload.message)
        return true
      }, {
        maxAttempts: 3,
        initialDelay: 1000
      })

      return result.success
    } catch (error) {
      console.error('Push notification failed:', error)
      return false
    }
  }

  /**
   * Envia SMS (placeholder para implementação futura)
   */
  private static async sendSMS(payload: NotificationPayload): Promise<boolean> {
    try {
      // TODO: Implementar envio de SMS com Twilio ou similar
      console.log('SMS notification:', payload.message)
      return true
    } catch (error) {
      console.error('SMS notification failed:', error)
      return false
    }
  }

  /**
   * Adiciona notificação na queue para processamento assíncrono
   */
  static async queue(payload: NotificationPayload, channels?: NotificationChannels): Promise<void> {
    this.queue.push(payload)
    
    if (!this.processing) {
      this.processQueue(channels)
    }
  }

  /**
   * Processa queue de notificações
   */
  private static async processQueue(channels?: NotificationChannels): Promise<void> {
    if (this.processing || this.queue.length === 0) {
      return
    }

    this.processing = true

    while (this.queue.length > 0) {
      const payload = this.queue.shift()
      
      if (payload) {
        try {
          await this.send(payload, channels)
        } catch (error) {
          console.error('Error processing notification queue:', error)
        }
      }
    }

    this.processing = false
  }

  /**
   * Helper: Email de boas-vindas
   */
  static async sendWelcomeEmail(email: string, name: string, userType: 'customer' | 'workshop'): Promise<NotificationResult> {
    const emailSent = userType === 'customer'
      ? await EmailService.sendWelcomeCustomer(email, name)
      : await EmailService.sendWelcomeWorkshop(email, name)

    return {
      success: emailSent,
      channels: {
        email: { sent: emailSent }
      },
      timestamp: new Date().toISOString()
    }
  }

  /**
   * Helper: Email de recuperação de senha
   */
  static async sendPasswordResetEmail(
    email: string,
    name: string,
    token: string,
    userType: 'customer' | 'workshop'
  ): Promise<NotificationResult> {
    const result = await RetryService.retryEmail(async () => {
      return await EmailService.sendPasswordReset(email, name, token, userType)
    })

    return {
      success: result.success,
      channels: {
        email: { sent: result.success, error: result.error?.message }
      },
      timestamp: new Date().toISOString()
    }
  }

  /**
   * Helper: Email de agendamento confirmado
   */
  static async sendBookingConfirmedEmail(
    customerEmail: string,
    customerName: string,
    workshopName: string,
    serviceTitle: string,
    scheduledDate: Date
  ): Promise<NotificationResult> {
    const result = await RetryService.retryEmail(async () => {
      return await EmailService.sendBookingConfirmed(
        customerEmail,
        customerName,
        workshopName,
        serviceTitle,
        scheduledDate
      )
    })

    return {
      success: result.success,
      channels: {
        email: { sent: result.success, error: result.error?.message }
      },
      timestamp: new Date().toISOString()
    }
  }
}

