import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { SERVICE_PHOTO_MODULE } from "../../../../../modules/service_photo"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * POST /store/bookings/:id/photos
 * 
 * Upload de fotos do serviço (antes/depois)
 * Oficina documenta o trabalho realizado
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { id: bookingId } = req.params
  const { photo_type, caption } = req.body
  
  const servicePhotoService = req.scope.resolve(SERVICE_PHOTO_MODULE)
  const fileService = req.scope.resolve(Modules.FILE)
  
  try {
    const files = req.files as Express.Multer.File[]
    
    if (!files || files.length === 0) {
      return res.status(400).json({
        message: "Nenhuma foto enviada"
      })
    }
    
    if (!photo_type) {
      return res.status(400).json({
        message: "Tipo de foto é obrigatório (before/after/parts/problem/inspection)"
      })
    }
    
    const uploadedPhotos = []
    
    for (const file of files) {
      // Validações
      if (file.size > 10 * 1024 * 1024) {
        return res.status(400).json({
          message: `Arquivo ${file.originalname} excede 10MB`
        })
      }
      
      if (!file.mimetype.startsWith('image/')) {
        return res.status(400).json({
          message: `Arquivo deve ser uma imagem`
        })
      }
      
      // Upload para S3
      const uploaded = await fileService.uploadFile({
        filename: `bookings/${bookingId}/${photo_type}/${Date.now()}-${file.originalname}`,
        mimeType: file.mimetype,
        content: file.buffer,
      })
      
      // Salvar no banco
      const photo = await servicePhotoService.createServicePhotos({
        booking_id: bookingId,
        workshop_id: req.body.workshop_id,
        photo_type,
        url: uploaded.url,
        caption,
        taken_at: new Date(),
        uploaded_by: req.body.uploaded_by || 'workshop',
        file_size: file.size,
        mime_type: file.mimetype,
      })
      
      uploadedPhotos.push(photo)
    }
    
    return res.status(201).json({
      message: `${uploadedPhotos.length} foto(s) enviada(s) com sucesso`,
      photos: uploadedPhotos
    })
    
  } catch (error) {
    console.error("Erro ao fazer upload de fotos:", error)
    
    return res.status(500).json({
      message: "Erro ao fazer upload",
      error: error.message
    })
  }
}

/**
 * GET /store/bookings/:id/photos
 * 
 * Lista todas as fotos de um agendamento
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { id: bookingId } = req.params
  const { photo_type } = req.query
  
  const servicePhotoService = req.scope.resolve(SERVICE_PHOTO_MODULE)
  
  try {
    const filters: any = { booking_id: bookingId }
    
    if (photo_type) {
      filters.photo_type = photo_type
    }
    
    const photos = await servicePhotoService.listServicePhotos(filters, {
      order: { taken_at: "ASC" }
    })
    
    // Agrupar por tipo
    const grouped = {
      before: photos.filter(p => p.photo_type === 'before'),
      after: photos.filter(p => p.photo_type === 'after'),
      parts: photos.filter(p => p.photo_type === 'parts'),
      problem: photos.filter(p => p.photo_type === 'problem'),
      inspection: photos.filter(p => p.photo_type === 'inspection'),
    }
    
    return res.json({
      booking_id: bookingId,
      total_photos: photos.length,
      photos,
      grouped
    })
    
  } catch (error) {
    console.error("Erro ao listar fotos:", error)
    return res.status(500).json({
      message: "Erro ao listar fotos",
      error: error.message
    })
  }
}

