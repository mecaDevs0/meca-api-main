import { model } from "@medusajs/framework/utils"

/**
 * Módulo MasterService - Lista Mestra de Serviços
 * 
 * Catálogo de serviços base que oficinas podem oferecer
 * Gerenciado pelo Admin
 */

export enum ServiceCategory {
  MANUTENCAO_PREVENTIVA = "manutencao_preventiva",
  MANUTENCAO_CORRETIVA = "manutencao_corretiva",
  TROCA_OLEO = "troca_oleo",
  FREIOS = "freios",
  SUSPENSAO = "suspensao",
  MOTOR = "motor",
  ELETRICA = "eletrica",
  AR_CONDICIONADO = "ar_condicionado",
  ALINHAMENTO_BALANCEAMENTO = "alinhamento_balanceamento",
  OUTROS = "outros"
}

const MasterService = model.define("master_service", {
  id: model.id().primaryKey(),
  
  // Informações Básicas
  name: model.text(),
  description: model.text().nullable(),
  category: model.enum(ServiceCategory),
  
  // Informações Técnicas
  estimated_duration_minutes: model.number().nullable(), // Duração estimada
  icon_url: model.text().nullable(),
  image_url: model.text().nullable(),
  
  // Preço Sugerido (orientação para oficinas)
  suggested_price_min: model.number().nullable(),
  suggested_price_max: model.number().nullable(),
  
  // Controle
  is_active: model.boolean().default(true),
  display_order: model.number().default(0),
})

export default MasterService

