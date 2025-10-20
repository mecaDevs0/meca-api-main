import { model } from "@medusajs/framework/utils"

/**
 * Service Photo - Fotos Antes/Depois do Serviço
 * Documentação visual do trabalho realizado
 */
export enum PhotoType {
  BEFORE = "before",
  AFTER = "after",
  PARTS = "parts", // Peças trocadas
  PROBLEM = "problem", // Problema encontrado
  INSPECTION = "inspection" // Inspeção geral
}

const ServicePhoto = model.define("service_photo", {
  id: model.id().primaryKey(),
  
  // Relacionamentos
  booking_id: model.text(),
  workshop_id: model.text(),
  
  // Detalhes da Foto
  photo_type: model.enum(PhotoType),
  url: model.text(), // URL da imagem no S3
  thumbnail_url: model.text().nullable(),
  
  // Metadados
  caption: model.text().nullable(), // Descrição/legenda
  taken_at: model.dateTime(),
  uploaded_by: model.text(), // ID do mecânico/oficina
  
  // Informações Técnicas
  file_size: model.number().nullable(),
  mime_type: model.text().nullable(),
  width: model.number().nullable(),
  height: model.number().nullable(),
})

export default ServicePhoto

