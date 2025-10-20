import { model } from "@medusajs/framework/utils"

/**
 * Modelo para armazenar dados de transações do PagBank
 */

export enum PagBankPaymentStatus {
  PENDING = "pending",
  AUTHORIZED = "authorized",
  PAID = "paid",
  DECLINED = "declined",
  REFUNDED = "refunded",
  CANCELLED = "cancelled"
}

const PagBankPayment = model.define("pagbank_payment", {
  id: model.id().primaryKey(),
  
  // Identificadores
  pagbank_transaction_id: model.text().nullable(),
  pagbank_charge_id: model.text().nullable(),
  
  // Dados da transação
  amount: model.number(),
  currency_code: model.text().default("brl"),
  
  // Status
  status: model.enum(PagBankPaymentStatus).default(PagBankPaymentStatus.PENDING),
  
  // Relação com Payment Session do Medusa
  payment_session_id: model.text(),
  
  // Metadata adicional do PagBank
  pagbank_response: model.json().nullable(),
  
  // Informações de reembolso
  refunded_at: model.dateTime().nullable(),
  refund_amount: model.number().nullable(),
})

export default PagBankPayment

