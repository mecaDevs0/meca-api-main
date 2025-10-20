import { model } from "@medusajs/framework/utils"

/**
 * Módulo Vehicle - Representa um veículo de um cliente
 * 
 * Cada cliente pode cadastrar múltiplos veículos.
 * Os veículos são usados no agendamento de serviços.
 */

const Vehicle = model.define("vehicle", {
  id: model.id().primaryKey(),
  
  // Informações do Veículo
  marca: model.text(), // ex: "Toyota"
  modelo: model.text(), // ex: "Corolla"
  ano: model.number(), // ex: 2020
  placa: model.text(), // ex: "ABC-1234"
  cor: model.text().nullable(),
  
  // Informações Adicionais
  km_atual: model.number().nullable(),
  combustivel: model.text().nullable(), // gasolina, etanol, diesel, flex, eletrico
  observacoes: model.text().nullable(),
  
  // Relação com Customer (será criada via Module Link)
  customer_id: model.text(),
  
  // created_at, updated_at, deleted_at são implícitos no Medusa
})

export default Vehicle

