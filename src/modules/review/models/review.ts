import { model } from "@medusajs/framework/utils"

/**
 * Módulo Review - Sistema de Avaliações
 * 
 * Permite clientes avaliarem oficinas e serviços após a conclusão
 */

const Review = model.define("review", {
  id: model.id().primaryKey(),
  
  // Relações
  customer_id: model.text(),
  oficina_id: model.text(),
  booking_id: model.text(), // Avaliação específica de um agendamento
  product_id: model.text().nullable(), // Serviço avaliado
  
  // Avaliação
  rating: model.number(), // 1 a 5 estrelas
  title: model.text().nullable(),
  comment: model.text().nullable(),
  
  // Resposta da Oficina
  oficina_response: model.text().nullable(),
  oficina_response_at: model.dateTime().nullable(),
  
  // Moderação
  is_approved: model.boolean().default(true),
  is_flagged: model.boolean().default(false),
  moderator_notes: model.text().nullable(),
})

export default Review

