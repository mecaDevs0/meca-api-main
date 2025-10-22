import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

/**
 * POST /store/upload
 * 
 * Upload de imagens para S3
 * Tipos suportados: logo_oficina, foto_oficina, foto_servico, avatar_cliente
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const fileService = req.scope.resolve(Modules.FILE)
  
  try {
    const files = req.files as Express.Multer.File[]
    
    if (!files || files.length === 0) {
      return res.status(400).json({
        message: "Nenhum arquivo enviado"
      })
    }
    
    const uploadedFiles = []
    
    for (const file of files) {
      // Validações
      const maxSize = 5 * 1024 * 1024 // 5MB
      if (file.size > maxSize) {
        return res.status(400).json({
          message: `Arquivo ${file.originalname} excede 5MB`
        })
      }
      
      const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp']
      if (!allowedTypes.includes(file.mimetype)) {
        return res.status(400).json({
          message: `Tipo de arquivo não permitido: ${file.mimetype}`
        })
      }
      
      // Upload para S3
      const uploadedFile = await fileService.uploadFile({
        filename: `${Date.now()}-${file.originalname}`,
        mimeType: file.mimetype,
        content: file.buffer,
      })
      
      uploadedFiles.push({
        url: uploadedFile.url,
        key: uploadedFile.key,
      })
    }
    
    return res.status(201).json({
      files: uploadedFiles
    })
    
  } catch (error) {
    console.error("Erro ao fazer upload:", error)
    
    return res.status(500).json({
      message: "Erro ao fazer upload",
      error: error.message
    })
  }
}

