import { GetObjectCommand, PutObjectCommand, S3Client } from "@aws-sdk/client-s3"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"
import fs from "fs"
import path from "path"
import { v4 as uuidv4 } from "uuid"

// Configuração do S3
const s3Client = new S3Client({
  region: process.env.AWS_REGION || "us-east-2",
  credentials: {
    accessKeyId: process.env.AWS_ACCESS_KEY_ID!,
    secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY!,
  },
})

const BUCKET_NAME = process.env.AWS_S3_BUCKET_NAME || "meca-secure-uploads"

// Tipos de arquivo permitidos com magic bytes
const ALLOWED_FILE_TYPES = {
  'image/jpeg': [0xFF, 0xD8, 0xFF],
  'image/png': [0x89, 0x50, 0x4E, 0x47],
  'image/gif': [0x47, 0x49, 0x46],
  'video/mp4': [0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70],
  'video/quicktime': [0x00, 0x00, 0x00, 0x14, 0x66, 0x74, 0x79, 0x70],
  'application/pdf': [0x25, 0x50, 0x44, 0x46]
}

const MAX_FILE_SIZE = 5 * 1024 * 1024 // 5MB
const MAX_VIDEO_SIZE = 50 * 1024 * 1024 // 50MB para vídeos

export class SecureUploadService {
  
  /**
   * Validar arquivo de forma segura
   */
  static async validateFile(filePath: string, mimeType: string): Promise<{
    isValid: boolean
    error?: string
    fileSize: number
  }> {
    try {
      // Verificar se arquivo existe
      if (!fs.existsSync(filePath)) {
        return { isValid: false, error: 'Arquivo não encontrado', fileSize: 0 }
      }

      // Verificar tamanho do arquivo
      const stats = fs.statSync(filePath)
      const fileSize = stats.size

      // Verificar limite de tamanho baseado no tipo
      const maxSize = mimeType.startsWith('video/') ? MAX_VIDEO_SIZE : MAX_FILE_SIZE
      if (fileSize > maxSize) {
        return { 
          isValid: false, 
          error: `Arquivo muito grande. Máximo: ${Math.round(maxSize / 1024 / 1024)}MB`, 
          fileSize 
        }
      }

      // Verificar magic bytes
      const buffer = fs.readFileSync(filePath, { start: 0, end: 10 })
      const isValidMagicBytes = this.validateMagicBytes(buffer, mimeType)
      
      if (!isValidMagicBytes) {
        return { isValid: false, error: 'Tipo de arquivo inválido ou corrompido', fileSize }
      }

      // Verificar se é um tipo permitido
      if (!ALLOWED_FILE_TYPES[mimeType as keyof typeof ALLOWED_FILE_TYPES]) {
        return { isValid: false, error: 'Tipo de arquivo não permitido', fileSize }
      }

      return { isValid: true, fileSize }

    } catch (error) {
      console.error('Erro na validação do arquivo:', error)
      return { isValid: false, error: 'Erro ao validar arquivo', fileSize: 0 }
    }
  }

  /**
   * Validar magic bytes do arquivo
   */
  private static validateMagicBytes(buffer: Buffer, mimeType: string): boolean {
    const expectedMagic = ALLOWED_FILE_TYPES[mimeType as keyof typeof ALLOWED_FILE_TYPES]
    if (!expectedMagic) return false

    for (let i = 0; i < expectedMagic.length; i++) {
      if (buffer[i] !== expectedMagic[i]) {
        return false
      }
    }
    return true
  }

  /**
   * Fazer upload seguro para S3
   */
  static async uploadSecureFile(
    filePath: string,
    mimeType: string,
    folder: string = 'uploads'
  ): Promise<{
    success: boolean
    fileUrl?: string
    fileName?: string
    error?: string
  }> {
    try {
      // Validar arquivo
      const validation = await this.validateFile(filePath, mimeType)
      if (!validation.isValid) {
        return { success: false, error: validation.error }
      }

      // Gerar nome único e seguro
      const fileExtension = path.extname(filePath)
      const uniqueId = uuidv4()
      const secureFileName = `${uniqueId}${fileExtension}`
      const s3Key = `${folder}/${secureFileName}`

      // Ler arquivo
      const fileBuffer = fs.readFileSync(filePath)

      // Upload para S3
      const uploadCommand = new PutObjectCommand({
        Bucket: BUCKET_NAME,
        Key: s3Key,
        Body: fileBuffer,
        ContentType: mimeType,
        ACL: 'private', // Arquivo privado
        Metadata: {
          'upload-timestamp': Date.now().toString(),
          'file-size': validation.fileSize.toString(),
          'secure-upload': 'true'
        }
      })

      await s3Client.send(uploadCommand)

      // Limpar arquivo temporário
      fs.unlinkSync(filePath)

      return {
        success: true,
        fileUrl: `s3://${BUCKET_NAME}/${s3Key}`,
        fileName: secureFileName
      }

    } catch (error) {
      console.error('Erro no upload seguro:', error)
      return { 
        success: false, 
        error: 'Erro interno no upload' 
      }
    }
  }

  /**
   * Gerar URL assinada para acesso seguro
   */
  static async generateSignedUrl(
    s3Key: string,
    expiresIn: number = 300 // 5 minutos
  ): Promise<{
    success: boolean
    signedUrl?: string
    error?: string
  }> {
    try {
      const command = new GetObjectCommand({
        Bucket: BUCKET_NAME,
        Key: s3Key,
      })

      const signedUrl = await getSignedUrl(s3Client, command, {
        expiresIn
      })

      return {
        success: true,
        signedUrl
      }

    } catch (error) {
      console.error('Erro ao gerar URL assinada:', error)
      return { 
        success: false, 
        error: 'Erro ao gerar URL assinada' 
      }
    }
  }

  /**
   * Processar múltiplos arquivos
   */
  static async uploadMultipleFiles(
    files: Array<{ path: string; mimeType: string }>,
    folder: string = 'uploads'
  ): Promise<{
    success: boolean
    results: Array<{
      success: boolean
      fileUrl?: string
      fileName?: string
      error?: string
    }>
  }> {
    const results = []

    for (const file of files) {
      const result = await this.uploadSecureFile(file.path, file.mimeType, folder)
      results.push(result)
    }

    const allSuccessful = results.every(r => r.success)

    return {
      success: allSuccessful,
      results
    }
  }

  /**
   * Verificar se arquivo é seguro (scan básico)
   */
  static async scanFile(filePath: string): Promise<{
    isSafe: boolean
    threats?: string[]
  }> {
    try {
      const buffer = fs.readFileSync(filePath)
      
      // Verificar padrões suspeitos
      const suspiciousPatterns = [
        /<script/i,
        /javascript:/i,
        /vbscript:/i,
        /onload=/i,
        /onerror=/i,
        /eval\(/i,
        /document\.cookie/i,
        /window\.location/i
      ]

      const content = buffer.toString('utf8', 0, Math.min(buffer.length, 1024))
      const threats = []

      for (const pattern of suspiciousPatterns) {
        if (pattern.test(content)) {
          threats.push(`Padrão suspeito detectado: ${pattern}`)
        }
      }

      // Verificar extensões perigosas em conteúdo
      const dangerousExtensions = ['.exe', '.bat', '.cmd', '.scr', '.pif', '.com']
      const fileName = path.basename(filePath).toLowerCase()
      
      for (const ext of dangerousExtensions) {
        if (content.includes(ext)) {
          threats.push(`Extensão perigosa detectada: ${ext}`)
        }
      }

      return {
        isSafe: threats.length === 0,
        threats: threats.length > 0 ? threats : undefined
      }

    } catch (error) {
      console.error('Erro no scan do arquivo:', error)
      return { isSafe: false, threats: ['Erro no scan do arquivo'] }
    }
  }

  /**
   * Limpar arquivos temporários
   */
  static async cleanupTempFiles(tempDir: string = 'uploads/temp'): Promise<void> {
    try {
      const files = fs.readdirSync(tempDir)
      const now = Date.now()
      const maxAge = 24 * 60 * 60 * 1000 // 24 horas

      for (const file of files) {
        const filePath = path.join(tempDir, file)
        const stats = fs.statSync(filePath)
        
        if (now - stats.mtime.getTime() > maxAge) {
          fs.unlinkSync(filePath)
          console.log(`Arquivo temporário removido: ${file}`)
        }
      }
    } catch (error) {
      console.error('Erro na limpeza de arquivos temporários:', error)
    }
  }
}













