import { model } from "@medusajs/framework/utils"

/**
 * Vehicle History - Histórico Completo de Manutenção
 * Cada entrada representa um serviço realizado
 */
const VehicleHistory = model.define("vehicle_history", {
  id: model.id().primaryKey(),
  
  // Relacionamentos
  vehicle_id: model.text(),
  booking_id: model.text(),
  service_name: model.text(),
  workshop_name: model.text(),
  
  // Detalhes do Serviço
  service_date: model.dateTime(),
  odometer_reading: model.number().nullable(), // KM do veículo
  next_service_km: model.number().nullable(), // Próxima manutenção em X km
  next_service_date: model.dateTime().nullable(),
  
  // Documentação
  notes: model.text().nullable(),
  parts_replaced: model.json().nullable(), // Array de peças trocadas
  labor_hours: model.number().nullable(),
  
  // Custos
  parts_cost: model.number(), // Custo de peças em centavos
  labor_cost: model.number(), // Custo de mão de obra
  total_cost: model.number(), // Custo total
  
  // Garantia
  warranty_months: model.number().default(3),
  warranty_expires_at: model.dateTime().nullable(),
})

export default VehicleHistory

