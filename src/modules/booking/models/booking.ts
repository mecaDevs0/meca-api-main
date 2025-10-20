import { model } from "@medusajs/framework/utils"

/**
 * Módulo Booking - Representa um Agendamento de Serviço
 * 
 * Esta é a entidade central do MECA que conecta:
 * - Cliente (Customer)
 * - Veículo (Vehicle)
 * - Oficina (Oficina)
 * - Serviço (Product)
 * - Pedido (Order)
 */

export enum BookingStatus {
  PENDENTE_OFICINA = "pendente_oficina",        // Aguardando confirmação da oficina
  CONFIRMADO = "confirmado",                     // Oficina confirmou o agendamento
  RECUSADO = "recusado",                         // Oficina recusou o agendamento
  FINALIZADO_MECANICO = "finalizado_mecanico",  // Mecânico finalizou o serviço
  FINALIZADO_CLIENTE = "finalizado_cliente",    // Cliente confirmou e pagou
  CANCELADO = "cancelado",                       // Cancelado pelo cliente ou oficina
  NAO_COMPARECEU = "nao_compareceu"             // Cliente não compareceu
}

const Booking = model.define("booking", {
  id: model.id().primaryKey(),
  
  // Data e Hora do Agendamento
  appointment_date: model.dateTime(),
  
  // Relações (serão criadas via Module Links)
  customer_id: model.text(),
  vehicle_id: model.text(),
  oficina_id: model.text(),
  product_id: model.text(),  // Serviço agendado
  order_id: model.text().nullable(),  // Pedido gerado a partir deste agendamento
  
  // Status do Agendamento
  status: model.enum(BookingStatus).default(BookingStatus.PENDENTE_OFICINA),
  status_history: model.json().nullable(), // histórico de mudanças de status
  
  // Informações do Veículo no momento do agendamento (snapshot)
  vehicle_snapshot: model.json().nullable(), // { marca, modelo, ano, placa, km }
  
  // Observações
  customer_notes: model.text().nullable(),  // Observações do cliente
  oficina_notes: model.text().nullable(),   // Observações da oficina
  
  // Informações de Preço (podem mudar após agendamento)
  estimated_price: model.number().nullable(),
  final_price: model.number().nullable(),
  
  // Timestamps Customizados (created_at, updated_at, deleted_at são implícitos)
  confirmed_at: model.dateTime().nullable(),
  completed_at: model.dateTime().nullable(),
  cancelled_at: model.dateTime().nullable(),
})

export default Booking

