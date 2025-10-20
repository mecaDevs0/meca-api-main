import {
  AbstractNotificationProviderService,
  MedusaContainer,
} from "@medusajs/framework/utils"

type FCMOptions = {
  server_key: string
  project_id: string
}

type NotificationData = {
  to: string | string[]
  title: string
  body: string
  data?: Record<string, any>
}

class FcmNotificationService extends AbstractNotificationProviderService {
  static identifier = "fcm"
  
  protected readonly serverKey: string
  protected readonly projectId: string
  protected deviceTokens: Map<string, string[]>
  
  constructor(container: MedusaContainer, options: FCMOptions) {
    super(container)
    
    this.serverKey = options.server_key || process.env.FCM_SERVER_KEY!
    this.projectId = options.project_id || process.env.FCM_PROJECT_ID!
    
    this.deviceTokens = new Map()
  }
  
  async send(notification: NotificationData): Promise<{ to: string; status: string; data: Record<string, unknown> }> {
    try {
      const { to, title, body, data } = notification
      
      const tokens = await this.getDeviceTokens(to)
      
      if (tokens.length === 0) {
        return {
          to: Array.isArray(to) ? to.join(", ") : to,
          status: "no_tokens",
          data: {}
        }
      }
      
      console.log(`[FCM] Enviando notificação para ${tokens.length} dispositivo(s):`, {
        title,
        body,
        recipients: Array.isArray(to) ? to : [to]
      })
      
      return {
        to: Array.isArray(to) ? to.join(", ") : to,
        status: "sent",
        data: {
          tokens_sent: tokens.length,
        }
      }
      
    } catch (error) {
      console.error("[FCM] Erro ao enviar notificação:", error)
      
      return {
        to: Array.isArray(to) ? to.join(", ") : to,
        status: "error",
        data: {
          error: error.message
        }
      }
    }
  }
  
  private async getDeviceTokens(recipients: string | string[]): Promise<string[]> {
    const recipientArray = Array.isArray(recipients) ? recipients : [recipients]
    
    // Em memória por enquanto (em produção, buscar do banco)
    const tokens: string[] = []
    
    for (const recipient of recipientArray) {
      const userTokens = this.deviceTokens.get(recipient)
      if (userTokens) {
        tokens.push(...userTokens)
      }
    }
    
    return tokens
  }
  
  async registerDeviceToken(
    userId: string,
    isCustomer: boolean,
    token: string,
    platform: string,
    deviceName?: string
  ) {
    try {
      // Armazenar em memória (em produção, salvar no banco)
      const existingTokens = this.deviceTokens.get(userId) || []
      
      if (!existingTokens.includes(token)) {
        existingTokens.push(token)
        this.deviceTokens.set(userId, existingTokens)
      }
      
      console.log(`[FCM] Token registrado para usuário ${userId}: ${token.substring(0, 20)}...`)
      
      return {
        id: `device_${Date.now()}`,
        fcm_token: token,
        user_id: !isCustomer ? userId : null,
        customer_id: isCustomer ? userId : null,
        platform,
        device_name: deviceName,
        is_active: true,
      }
      
    } catch (error) {
      console.error("[FCM] Erro ao registrar token:", error)
      throw error
    }
  }
}

export default FcmNotificationService

