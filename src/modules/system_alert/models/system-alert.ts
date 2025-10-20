import { model } from "@medusajs/framework/utils"

/**
 * System Alert - Alertas Automáticos do Sistema
 * Monitora anomalias e eventos importantes
 */
export enum AlertType {
  ANOMALY = "anomaly", // Comportamento anômalo
  WARNING = "warning", // Aviso
  ERROR = "error", // Erro crítico
  INFO = "info", // Informação
  SECURITY = "security" // Segurança
}

export enum AlertSeverity {
  LOW = "low",
  MEDIUM = "medium",
  HIGH = "high",
  CRITICAL = "critical"
}

export enum AlertCategory {
  WORKSHOP = "workshop", // Relacionado a oficinas
  CLIENT = "client", // Relacionado a clientes
  BOOKING = "booking", // Relacionado a agendamentos
  PAYMENT = "payment", // Relacionado a pagamentos
  REVIEW = "review", // Relacionado a avaliações
  SYSTEM = "system" // Sistema geral
}

const SystemAlert = model.define("system_alert", {
  id: model.id().primaryKey(),
  
  // Classificação
  type: model.enum(AlertType),
  severity: model.enum(AlertSeverity),
  category: model.enum(AlertCategory),
  
  // Conteúdo
  title: model.text(),
  message: model.text(),
  details: model.json().nullable(), // Dados adicionais
  
  // Contexto
  entity_type: model.text().nullable(), // workshop, client, booking
  entity_id: model.text().nullable(),
  
  // Ações
  action_required: model.boolean().default(false),
  action_url: model.text().nullable(), // URL para resolver
  
  // Controle
  is_read: model.boolean().default(false),
  is_resolved: model.boolean().default(false),
  resolved_at: model.dateTime().nullable(),
  resolved_by: model.text().nullable(),
  
  // Automação
  auto_generated: model.boolean().default(true),
  triggered_by: model.text().nullable(), // Regra que gerou o alerta
})

export default SystemAlert

