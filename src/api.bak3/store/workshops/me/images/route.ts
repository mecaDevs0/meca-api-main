import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * POST /store/workshops/me/images
 * 
 * Upload de imagens para a oficina (logo, fotos de perfil, fotos de serviços)
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { image_type, image_data, service_id } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  if (!image_type || !image_data) {
    return res.status(400).json({
      message: "image_type e image_data são obrigatórios"
    })
  }
  
  // Tipos de imagem permitidos
  const allowedTypes = ['logo', 'profile_photo', 'service_photo']
  if (!allowedTypes.includes(image_type)) {
    return res.status(400).json({
      message: "image_type deve ser: logo, profile_photo ou service_photo"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    const currentMetadata = oficinas[0].metadata || {}
    
    // Validar se é base64 válido
    if (!image_data.startsWith('data:image/')) {
      return res.status(400).json({
        message: "image_data deve ser um base64 válido começando com 'data:image/'"
      })
    }
    
    // Preparar dados da imagem
    const imageInfo = {
      data: image_data,
      uploaded_at: new Date().toISOString(),
      type: image_type,
      size: image_data.length, // Tamanho aproximado em bytes
    }
    
    let updatedMetadata = { ...currentMetadata }
    
    // Atualizar metadata baseado no tipo de imagem
    switch (image_type) {
      case 'logo':
        updatedMetadata.logo_image = imageInfo
        break
      case 'profile_photo':
        updatedMetadata.profile_photos = updatedMetadata.profile_photos || []
        updatedMetadata.profile_photos.push(imageInfo)
        break
      case 'service_photo':
        if (!service_id) {
          return res.status(400).json({
            message: "service_id é obrigatório para service_photo"
          })
        }
        updatedMetadata.service_photos = updatedMetadata.service_photos || {}
        updatedMetadata.service_photos[service_id] = updatedMetadata.service_photos[service_id] || []
        updatedMetadata.service_photos[service_id].push(imageInfo)
        break
    }
    
    // Atualizar oficina
    const updatedOficina = await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      metadata: updatedMetadata
    }])
    
    return res.json({
      success: true,
      message: "Imagem salva com sucesso",
      data: {
        image_type,
        uploaded_at: imageInfo.uploaded_at,
        size: imageInfo.size
      }
    })
    
  } catch (error) {
    console.error("Erro ao salvar imagem:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao salvar imagem",
      error: error.message
    })
  }
}

/**
 * GET /store/workshops/me/images
 * 
 * Buscar imagens da oficina
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { image_type, service_id } = req.query
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficina = oficinas[0]
    const metadata = oficina.metadata || {}
    
    let images = {}
    
    // Retornar imagens baseado no filtro
    if (image_type) {
      switch (image_type) {
        case 'logo':
          images.logo = metadata.logo_image || null
          break
        case 'profile_photos':
          images.profile_photos = metadata.profile_photos || []
          break
        case 'service_photos':
          if (service_id) {
            images.service_photos = metadata.service_photos?.[service_id] || []
          } else {
            images.service_photos = metadata.service_photos || {}
          }
          break
        default:
          return res.status(400).json({
            message: "image_type deve ser: logo, profile_photos ou service_photos"
          })
      }
    } else {
      // Retornar todas as imagens
      images = {
        logo: metadata.logo_image || null,
        profile_photos: metadata.profile_photos || [],
        service_photos: metadata.service_photos || {}
      }
    }
    
    return res.json({
      success: true,
      data: images
    })
    
  } catch (error) {
    console.error("Erro ao buscar imagens:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar imagens",
      error: error.message
    })
  }
}

/**
 * DELETE /store/workshops/me/images
 * 
 * Deletar imagem da oficina
 */
export async function DELETE(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { image_type, image_index, service_id } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  if (!image_type) {
    return res.status(400).json({
      message: "image_type é obrigatório"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    const currentMetadata = oficinas[0].metadata || {}
    let updatedMetadata = { ...currentMetadata }
    
    // Deletar imagem baseado no tipo
    switch (image_type) {
      case 'logo':
        delete updatedMetadata.logo_image
        break
      case 'profile_photos':
        if (image_index !== undefined) {
          updatedMetadata.profile_photos = updatedMetadata.profile_photos || []
          updatedMetadata.profile_photos.splice(image_index, 1)
        } else {
          updatedMetadata.profile_photos = []
        }
        break
      case 'service_photos':
        if (!service_id) {
          return res.status(400).json({
            message: "service_id é obrigatório para service_photos"
          })
        }
        if (image_index !== undefined) {
          updatedMetadata.service_photos = updatedMetadata.service_photos || {}
          updatedMetadata.service_photos[service_id] = updatedMetadata.service_photos[service_id] || []
          updatedMetadata.service_photos[service_id].splice(image_index, 1)
        } else {
          updatedMetadata.service_photos = updatedMetadata.service_photos || {}
          delete updatedMetadata.service_photos[service_id]
        }
        break
      default:
        return res.status(400).json({
          message: "image_type deve ser: logo, profile_photos ou service_photos"
        })
    }
    
    // Atualizar oficina
    const updatedOficina = await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      metadata: updatedMetadata
    }])
    
    return res.json({
      success: true,
      message: "Imagem deletada com sucesso"
    })
    
  } catch (error) {
    console.error("Erro ao deletar imagem:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao deletar imagem",
      error: error.message
    })
  }
}
















