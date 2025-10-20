import { model } from "@medusajs/framework/utils"

/**
 * Módulo Oficina - Representa uma Oficina parceira no marketplace MECA
 * 
 * Estende o conceito de Store do Medusa para adicionar campos específicos
 * de oficinas automotivas como CNPJ, dados bancários, horário de funcionamento, etc.
 */

export enum OficinaStatus {
  PENDENTE = "pendente",
  APROVADO = "aprovado",
  REJEITADO = "rejeitado",
  SUSPENSO = "suspenso"
}

const Oficina = model.define("oficina", {
  id: model.id().primaryKey(),
  
  // Informações Básicas
  name: model.text(),
  cnpj: model.text().unique(),
  email: model.text(),
  phone: model.text(),
  
  // Endereço completo
  address: model.json().nullable(), // { rua, numero, bairro, cidade, estado, cep, lat, lng }
  
  // Informações Visuais
  logo_url: model.text().nullable(),
  photo_urls: model.json().nullable(), // array de URLs de fotos da oficina
  
  // Informações Operacionais
  description: model.text().nullable(),
  dados_bancarios: model.json().nullable(), // { banco, agencia, conta, tipo_conta, titular }
  horario_funcionamento: model.json().nullable(), // { seg: { inicio: "08:00", fim: "18:00" }, ... }
  
  // Status de Aprovação (fluxo de onboarding)
  status: model.enum(OficinaStatus).default(OficinaStatus.PENDENTE),
  status_reason: model.text().nullable(), // motivo de rejeição/suspensão
  
  // Metadados adicionais (inclui password_hash para autenticação)
  metadata: model.json().nullable(),
  
  // created_at, updated_at, deleted_at são implícitos no Medusa
})

export default Oficina

