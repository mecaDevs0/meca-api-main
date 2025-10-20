import axios, { AxiosInstance } from 'axios'

export interface PagBankConfig {
  accessToken: string
  baseUrl: string
  isProduction: boolean
}

export interface PagBankResponse<T> {
  success: boolean
  data?: T
  error?: string
  message?: string
}

export interface PagBankAccount {
  id: string
  name: string
  email: string
  document: string
  type: 'individual' | 'company'
  status: 'pending' | 'approved' | 'rejected' | 'suspended'
  created_at: string
  updated_at: string
}

export interface PagBankAccountRequest {
  name: string
  email: string
  document: string
  type: 'individual' | 'company'
  address: PagBankAddress
  phone: PagBankPhone
}

export interface PagBankAddress {
  street: string
  number: string
  complement?: string
  neighborhood: string
  city: string
  state: string
  zip_code: string
  country: string
}

export interface PagBankPhone {
  country: string
  area: string
  number: string
}

export interface PagBankBankAccount {
  id: string
  account_number: string
  bank_code: string
  agency_number: string
  holder_name: string
  holder_type: 'individual' | 'company'
  status: 'pending' | 'approved' | 'rejected'
  created_at: string
}

export interface PagBankBankAccountRequest {
  account_number: string
  bank_code: string
  agency_number: string
  holder_name: string
  holder_type: 'individual' | 'company'
}

export interface PagBankPayment {
  id: string
  amount: number
  currency: string
  status: 'pending' | 'approved' | 'rejected' | 'cancelled'
  payment_method: string
  created_at: string
  updated_at: string
}

export interface PagBankPaymentRequest {
  amount: number
  currency: string
  payment_method: string
  customer_id: string
  description?: string
}

export class PagBankService {
  private httpClient: AxiosInstance
  private config: PagBankConfig

  constructor(config: PagBankConfig) {
    this.config = config
    
    this.httpClient = axios.create({
      baseURL: config.baseUrl,
      timeout: 30000,
      headers: {
        'Authorization': `Bearer ${config.accessToken}`,
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    })

    // Interceptor para tratamento de erros
    this.httpClient.interceptors.response.use(
      (response) => response,
      (error) => {
        console.error('PagBank API Error:', error.response?.data || error.message)
        return Promise.reject(error)
      }
    )
  }

  private async handleRequest<T>(
    request: () => Promise<T>
  ): Promise<PagBankResponse<T>> {
    try {
      const data = await request()
      return {
        success: true,
        data
      }
    } catch (error: any) {
      return {
        success: false,
        error: error.response?.data?.message || error.message,
        message: 'Erro na comunicação com PagBank'
      }
    }
  }

  // Criar conta no PagBank
  async createAccount(request: PagBankAccountRequest): Promise<PagBankResponse<PagBankAccount>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.post('/accounts', request)
      return response.data
    })
  }

  // Buscar conta por ID
  async getAccount(accountId: string): Promise<PagBankResponse<PagBankAccount>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.get(`/accounts/${accountId}`)
      return response.data
    })
  }

  // Atualizar conta
  async updateAccount(
    accountId: string, 
    request: Partial<PagBankAccountRequest>
  ): Promise<PagBankResponse<PagBankAccount>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.put(`/accounts/${accountId}`, request)
      return response.data
    })
  }

  // Criar conta bancária
  async createBankAccount(
    accountId: string, 
    request: PagBankBankAccountRequest
  ): Promise<PagBankResponse<PagBankBankAccount>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.post(`/accounts/${accountId}/bank-accounts`, request)
      return response.data
    })
  }

  // Buscar contas bancárias
  async getBankAccounts(accountId: string): Promise<PagBankResponse<PagBankBankAccount[]>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.get(`/accounts/${accountId}/bank-accounts`)
      return response.data.items || []
    })
  }

  // Criar pagamento
  async createPayment(request: PagBankPaymentRequest): Promise<PagBankResponse<PagBankPayment>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.post('/payments', request)
      return response.data
    })
  }

  // Buscar pagamento por ID
  async getPayment(paymentId: string): Promise<PagBankResponse<PagBankPayment>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.get(`/payments/${paymentId}`)
      return response.data
    })
  }

  // Listar pagamentos
  async listPayments(
    accountId: string,
    limit: number = 50,
    offset: number = 0
  ): Promise<PagBankResponse<PagBankPayment[]>> {
    return this.handleRequest(async () => {
      const response = await this.httpClient.get(`/accounts/${accountId}/payments`, {
        params: { limit, offset }
      })
      return response.data.items || []
    })
  }

  // Webhook para receber notificações do PagBank
  async handleWebhook(payload: any, signature: string): Promise<PagBankResponse<any>> {
    return this.handleRequest(async () => {
      // Validar assinatura do webhook
      // Em produção, você deve validar a assinatura para garantir que a notificação veio do PagBank
      
      const eventType = payload.event_type
      const eventData = payload.data

      switch (eventType) {
        case 'payment.approved':
          // Pagamento aprovado
          console.log('Pagamento aprovado:', eventData)
          break
        case 'payment.rejected':
          // Pagamento rejeitado
          console.log('Pagamento rejeitado:', eventData)
          break
        case 'payment.cancelled':
          // Pagamento cancelado
          console.log('Pagamento cancelado:', eventData)
          break
        default:
          console.log('Evento não tratado:', eventType, eventData)
      }

      return { received: true, event_type: eventType }
    })
  }

  // Utilitários
  static createConfig(accessToken: string, isProduction: boolean = false): PagBankConfig {
    return {
      accessToken,
      baseUrl: isProduction 
        ? 'https://api.pagseguro.com' 
        : 'https://sandbox.api.pagseguro.com',
      isProduction
    }
  }

  // Validar CPF/CNPJ
  static validateDocument(document: string): boolean {
    // Remove caracteres não numéricos
    const cleanDoc = document.replace(/\D/g, '')
    
    if (cleanDoc.length === 11) {
      // Validar CPF
      return this.validateCPF(cleanDoc)
    } else if (cleanDoc.length === 14) {
      // Validar CNPJ
      return this.validateCNPJ(cleanDoc)
    }
    
    return false
  }

  private static validateCPF(cpf: string): boolean {
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
      return false
    }

    let sum = 0
    for (let i = 0; i < 9; i++) {
      sum += parseInt(cpf.charAt(i)) * (10 - i)
    }
    let remainder = (sum * 10) % 11
    if (remainder === 10 || remainder === 11) remainder = 0
    if (remainder !== parseInt(cpf.charAt(9))) return false

    sum = 0
    for (let i = 0; i < 10; i++) {
      sum += parseInt(cpf.charAt(i)) * (11 - i)
    }
    remainder = (sum * 10) % 11
    if (remainder === 10 || remainder === 11) remainder = 0
    if (remainder !== parseInt(cpf.charAt(10))) return false

    return true
  }

  private static validateCNPJ(cnpj: string): boolean {
    if (cnpj.length !== 14 || /^(\d)\1{13}$/.test(cnpj)) {
      return false
    }

    let sum = 0
    let weight = 2
    for (let i = 11; i >= 0; i--) {
      sum += parseInt(cnpj.charAt(i)) * weight
      weight = weight === 9 ? 2 : weight + 1
    }
    let remainder = sum % 11
    const firstDigit = remainder < 2 ? 0 : 11 - remainder
    if (firstDigit !== parseInt(cnpj.charAt(12))) return false

    sum = 0
    weight = 2
    for (let i = 12; i >= 0; i--) {
      sum += parseInt(cnpj.charAt(i)) * weight
      weight = weight === 9 ? 2 : weight + 1
    }
    remainder = sum % 11
    const secondDigit = remainder < 2 ? 0 : 11 - remainder
    if (secondDigit !== parseInt(cnpj.charAt(13))) return false

    return true
  }
}

// Instância singleton do serviço
let pagBankServiceInstance: PagBankService | null = null

export function getPagBankService(): PagBankService {
  if (!pagBankServiceInstance) {
    // Token real do PagBank fornecido pelo usuário
    const accessToken = '987fa5bc-900b-4903-8172-20f83def1c8b7192a70d4be7bbdd8d2763bff311c2fd93ae-8354-4998-84e6-b5262574d9bd'
    
    const config = PagBankService.createConfig(accessToken, false) // Usando sandbox para testes
    pagBankServiceInstance = new PagBankService(config)
  }
  
  return pagBankServiceInstance
}
