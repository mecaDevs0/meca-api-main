import crypto from "crypto"
import { EntityManager } from "typeorm"

// Configurações do PagBank para tokenização
const PAGBANK_API_URL = process.env.PAGBANK_API_URL || 'https://api.pagseguro.com'
const PAGBANK_TOKEN = process.env.PAGBANK_TOKEN

export class CardTokenizationService {

  /**
   * Tokenizar cartão de crédito via PagBank
   * NUNCA armazena dados sensíveis no nosso servidor
   */
  static async tokenizeCard(cardData: {
    number: string
    expiryMonth: string
    expiryYear: string
    cvv: string
    holderName: string
  }): Promise<{
    success: boolean
    token?: string
    cardInfo?: {
      brand: string
      lastFourDigits: string
      holderName: string
    }
    error?: string
  }> {
    try {
      console.log('🔐 Iniciando tokenização de cartão...')

      // Validar dados do cartão
      const validation = this.validateCardData(cardData)
      if (!validation.isValid) {
        return { success: false, error: validation.error }
      }

      // Chamar API do PagBank para tokenização
      const response = await fetch(`${PAGBANK_API_URL}/cards/tokenize`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${PAGBANK_TOKEN}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          card_number: cardData.number,
          expiration_month: cardData.expiryMonth,
          expiration_year: cardData.expiryYear,
          security_code: cardData.cvv,
          holder_name: cardData.holderName
        })
      })

      if (!response.ok) {
        const errorData = await response.json()
        return { 
          success: false, 
          error: errorData.message || 'Erro na tokenização' 
        }
      }

      const tokenData = await response.json()

      return {
        success: true,
        token: tokenData.token,
        cardInfo: {
          brand: tokenData.brand,
          lastFourDigits: tokenData.last_four_digits,
          holderName: cardData.holderName
        }
      }

    } catch (error) {
      console.error('❌ Erro na tokenização:', error)
      return { 
        success: false, 
        error: 'Erro interno na tokenização' 
      }
    }
  }

  /**
   * Salvar cartão tokenizado no banco (apenas token, nunca dados sensíveis)
   */
  static async saveTokenizedCard(
    manager: EntityManager,
    customerId: string,
    token: string,
    cardInfo: {
      brand: string
      lastFourDigits: string
      holderName: string
    },
    isDefault: boolean = false
  ): Promise<{
    success: boolean
    cardId?: string
    error?: string
  }> {
    try {
      // Verificar se já existe cartão com mesmo token
      const existingCard = await manager.query(`
        SELECT id FROM saved_cards 
        WHERE card_token = $1 AND customer_id = $2
      `, [token, customerId])

      if (existingCard.length > 0) {
        return { 
          success: false, 
          error: 'Cartão já cadastrado' 
        }
      }

      // Se for padrão, remover padrão dos outros cartões
      if (isDefault) {
        await manager.query(`
          UPDATE saved_cards 
          SET is_default = FALSE 
          WHERE customer_id = $1
        `, [customerId])
      }

      // Inserir novo cartão tokenizado
      const cardId = `card_${Date.now()}_${crypto.randomBytes(8).toString('hex')}`
      
      await manager.query(`
        INSERT INTO saved_cards (
          id, customer_id, card_token, card_brand, 
          last_four_digits, cardholder_name, is_default, 
          active, created_at, updated_at
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)
      `, [
        cardId,
        customerId,
        token,
        cardInfo.brand,
        cardInfo.lastFourDigits,
        cardInfo.holderName,
        isDefault,
        true,
        new Date(),
        new Date()
      ])

      console.log(`✅ Cartão tokenizado salvo: ${cardId}`)

      return {
        success: true,
        cardId
      }

    } catch (error) {
      console.error('❌ Erro ao salvar cartão tokenizado:', error)
      return { 
        success: false, 
        error: 'Erro interno ao salvar cartão' 
      }
    }
  }

  /**
   * Obter cartões salvos do cliente
   */
  static async getSavedCards(
    manager: EntityManager,
    customerId: string
  ): Promise<{
    success: boolean
    cards?: Array<{
      id: string
      brand: string
      lastFourDigits: string
      holderName: string
      isDefault: boolean
      active: boolean
    }>
    error?: string
  }> {
    try {
      const cards = await manager.query(`
        SELECT 
          id, card_brand, last_four_digits, 
          cardholder_name, is_default, active
        FROM saved_cards 
        WHERE customer_id = $1 AND active = TRUE
        ORDER BY is_default DESC, created_at DESC
      `, [customerId])

      return {
        success: true,
        cards: cards.map(card => ({
          id: card.id,
          brand: card.card_brand,
          lastFourDigits: card.last_four_digits,
          holderName: card.cardholder_name,
          isDefault: card.is_default,
          active: card.active
        }))
      }

    } catch (error) {
      console.error('❌ Erro ao buscar cartões salvos:', error)
      return { 
        success: false, 
        error: 'Erro interno ao buscar cartões' 
      }
    }
  }

  /**
   * Definir cartão como padrão
   */
  static async setDefaultCard(
    manager: EntityManager,
    customerId: string,
    cardId: string
  ): Promise<{
    success: boolean
    error?: string
  }> {
    try {
      // Verificar se cartão pertence ao cliente
      const card = await manager.query(`
        SELECT id FROM saved_cards 
        WHERE id = $1 AND customer_id = $2 AND active = TRUE
      `, [cardId, customerId])

      if (card.length === 0) {
        return { 
          success: false, 
          error: 'Cartão não encontrado' 
        }
      }

      // Remover padrão dos outros cartões
      await manager.query(`
        UPDATE saved_cards 
        SET is_default = FALSE 
        WHERE customer_id = $1
      `, [customerId])

      // Definir novo cartão como padrão
      await manager.query(`
        UPDATE saved_cards 
        SET is_default = TRUE, updated_at = $1
        WHERE id = $2
      `, [new Date(), cardId])

      console.log(`✅ Cartão ${cardId} definido como padrão`)

      return { success: true }

    } catch (error) {
      console.error('❌ Erro ao definir cartão padrão:', error)
      return { 
        success: false, 
        error: 'Erro interno ao definir cartão padrão' 
      }
    }
  }

  /**
   * Remover cartão salvo
   */
  static async removeCard(
    manager: EntityManager,
    customerId: string,
    cardId: string
  ): Promise<{
    success: boolean
    error?: string
  }> {
    try {
      // Verificar se cartão pertence ao cliente
      const card = await manager.query(`
        SELECT id FROM saved_cards 
        WHERE id = $1 AND customer_id = $2
      `, [cardId, customerId])

      if (card.length === 0) {
        return { 
          success: false, 
          error: 'Cartão não encontrado' 
        }
      }

      // Marcar como inativo (não deletar por segurança)
      await manager.query(`
        UPDATE saved_cards 
        SET active = FALSE, updated_at = $1
        WHERE id = $2
      `, [new Date(), cardId])

      console.log(`✅ Cartão ${cardId} removido`)

      return { success: true }

    } catch (error) {
      console.error('❌ Erro ao remover cartão:', error)
      return { 
        success: false, 
        error: 'Erro interno ao remover cartão' 
      }
    }
  }

  /**
   * Processar pagamento com cartão tokenizado
   */
  static async processPaymentWithToken(
    token: string,
    amount: number,
    bookingId: string,
    installments: number = 1
  ): Promise<{
    success: boolean
    paymentId?: string
    error?: string
  }> {
    try {
      console.log(`💳 Processando pagamento com token: ${amount}`)

      const response = await fetch(`${PAGBANK_API_URL}/payments`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${PAGBANK_TOKEN}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          amount: Math.round(amount * 100), // Converter para centavos
          payment_method: 'credit_card',
          card_token: token,
          installments: installments,
          metadata: {
            booking_id: bookingId,
            source: 'meca_app'
          }
        })
      })

      if (!response.ok) {
        const errorData = await response.json()
        return { 
          success: false, 
          error: errorData.message || 'Erro no processamento do pagamento' 
        }
      }

      const paymentData = await response.json()

      return {
        success: true,
        paymentId: paymentData.id
      }

    } catch (error) {
      console.error('❌ Erro no processamento do pagamento:', error)
      return { 
        success: false, 
        error: 'Erro interno no processamento' 
      }
    }
  }

  /**
   * Validar dados do cartão
   */
  private static validateCardData(cardData: {
    number: string
    expiryMonth: string
    expiryYear: string
    cvv: string
    holderName: string
  }): { isValid: boolean; error?: string } {
    
    // Validar número do cartão (Luhn algorithm)
    if (!this.validateCardNumber(cardData.number)) {
      return { isValid: false, error: 'Número do cartão inválido' }
    }

    // Validar mês de expiração
    const month = parseInt(cardData.expiryMonth)
    if (month < 1 || month > 12) {
      return { isValid: false, error: 'Mês de expiração inválido' }
    }

    // Validar ano de expiração
    const year = parseInt(cardData.expiryYear)
    const currentYear = new Date().getFullYear()
    if (year < currentYear || year > currentYear + 20) {
      return { isValid: false, error: 'Ano de expiração inválido' }
    }

    // Validar CVV
    if (!/^\d{3,4}$/.test(cardData.cvv)) {
      return { isValid: false, error: 'CVV inválido' }
    }

    // Validar nome do portador
    if (!cardData.holderName || cardData.holderName.trim().length < 2) {
      return { isValid: false, error: 'Nome do portador inválido' }
    }

    return { isValid: true }
  }

  /**
   * Validar número do cartão usando algoritmo de Luhn
   */
  private static validateCardNumber(number: string): boolean {
    const cleanNumber = number.replace(/\D/g, '')
    
    if (cleanNumber.length < 13 || cleanNumber.length > 19) {
      return false
    }

    let sum = 0
    let isEven = false

    for (let i = cleanNumber.length - 1; i >= 0; i--) {
      let digit = parseInt(cleanNumber[i])

      if (isEven) {
        digit *= 2
        if (digit > 9) {
          digit -= 9
        }
      }

      sum += digit
      isEven = !isEven
    }

    return sum % 10 === 0
  }
}













