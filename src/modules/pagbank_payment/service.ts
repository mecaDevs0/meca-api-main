import {
  AbstractPaymentProvider,
  PaymentProviderError,
  PaymentProviderSessionResponse,
  PaymentSessionStatus,
  WebhookActionResult,
} from "@medusajs/framework/utils"

/**
 * PagBank Payment Provider
 * 
 * Provedor customizado para integração com PagBank (PagSeguro)
 */

type PagBankOptions = {
  api_key: string
  api_secret: string
  environment: "sandbox" | "production"
}

class PagBankProviderService extends AbstractPaymentProvider<PagBankOptions> {
  static identifier = "pagbank"
  
  protected readonly pagbankApiUrl: string
  protected readonly apiKey: string
  protected readonly apiSecret: string
  protected paymentSessions: Map<string, any>
  
  constructor(container, options: PagBankOptions) {
    super(container, options)
    
    this.apiKey = options.api_key || process.env.PAGBANK_API_KEY!
    this.apiSecret = options.api_secret || process.env.PAGBANK_API_SECRET!
    
    this.pagbankApiUrl = options.environment === "production"
      ? "https://api.pagseguro.com"
      : "https://sandbox.api.pagseguro.com"
    
    this.paymentSessions = new Map()
  }
  
  async getPaymentData(context): Promise<Record<string, unknown>> {
    return {
      api_key: this.apiKey,
      environment: this.options_.environment,
    }
  }
  
  async initiatePayment(context): Promise<PaymentProviderError | PaymentProviderSessionResponse> {
    try {
      const { amount, currency_code, context: paymentContext } = context
      
      const sessionId = `pagbank_session_${Date.now()}`
      
      const pagbankResponse = {
        id: sessionId,
        transaction_id: `pagbank_txn_${Date.now()}`,
        charge_id: `pagbank_charge_${Date.now()}`,
        checkout_url: `${this.pagbankApiUrl}/checkout/${sessionId}`,
        amount,
        currency_code,
        status: "pending",
      }
      
      // Armazenar sessão em memória (em produção, usar Redis ou banco)
      this.paymentSessions.set(sessionId, pagbankResponse)
      
      return {
        data: pagbankResponse,
      }
      
    } catch (error) {
      return {
        error: error.message,
        code: "PAGBANK_INITIATE_ERROR",
        detail: error,
      }
    }
  }
  
  async authorizePayment(paymentSessionData, context): Promise<PaymentProviderError | PaymentProviderSessionResponse> {
    try {
      const session = this.paymentSessions.get(paymentSessionData.id)
      
      if (session) {
        session.status = "authorized"
        this.paymentSessions.set(paymentSessionData.id, session)
      }
      
      return {
        status: PaymentSessionStatus.AUTHORIZED,
        data: paymentSessionData,
      }
      
    } catch (error) {
      return {
        error: error.message,
        code: "PAGBANK_AUTHORIZE_ERROR",
        detail: error,
      }
    }
  }
  
  async capturePayment(paymentData): Promise<PaymentProviderError | PaymentProviderSessionResponse["data"]> {
    try {
      const session = this.paymentSessions.get(paymentData.id)
      
      if (session) {
        session.status = "paid"
        this.paymentSessions.set(paymentData.id, session)
      }
      
      return paymentData
      
    } catch (error) {
      return {
        error: error.message,
        code: "PAGBANK_CAPTURE_ERROR",
        detail: error,
      }
    }
  }
  
  async cancelPayment(paymentData): Promise<PaymentProviderError | PaymentProviderSessionResponse["data"]> {
    try {
      const session = this.paymentSessions.get(paymentData.id)
      
      if (session) {
        session.status = "cancelled"
        this.paymentSessions.set(paymentData.id, session)
      }
      
      return paymentData
      
    } catch (error) {
      return {
        error: error.message,
        code: "PAGBANK_CANCEL_ERROR",
        detail: error,
      }
    }
  }
  
  async refundPayment(paymentData, refundAmount): Promise<PaymentProviderError | PaymentProviderSessionResponse["data"]> {
    try {
      const session = this.paymentSessions.get(paymentData.id)
      
      if (session) {
        session.status = "refunded"
        session.refund_amount = refundAmount
        session.refunded_at = new Date().toISOString()
        this.paymentSessions.set(paymentData.id, session)
      }
      
      return paymentData
      
    } catch (error) {
      return {
        error: error.message,
        code: "PAGBANK_REFUND_ERROR",
        detail: error,
      }
    }
  }
  
  async getPaymentStatus(paymentData): Promise<PaymentSessionStatus> {
    try {
      const session = this.paymentSessions.get(paymentData.id)
      
      if (!session) {
        return PaymentSessionStatus.PENDING
      }
      
      const statusMap = {
        "pending": PaymentSessionStatus.PENDING,
        "authorized": PaymentSessionStatus.AUTHORIZED,
        "paid": PaymentSessionStatus.AUTHORIZED,
        "declined": PaymentSessionStatus.ERROR,
        "refunded": PaymentSessionStatus.CANCELED,
        "cancelled": PaymentSessionStatus.CANCELED,
      }
      
      return statusMap[session.status] || PaymentSessionStatus.PENDING
      
    } catch (error) {
      return PaymentSessionStatus.ERROR
    }
  }
  
  async updatePayment(context): Promise<PaymentProviderError | PaymentProviderSessionResponse> {
    return {
      data: context.data,
    }
  }
  
  async deletePayment(paymentData): Promise<PaymentProviderError | PaymentProviderSessionResponse["data"]> {
    return paymentData
  }
  
  async getWebhookActionAndData(data): Promise<WebhookActionResult> {
    return {
      action: "not_supported" as any,
    }
  }
}

export default PagBankProviderService

