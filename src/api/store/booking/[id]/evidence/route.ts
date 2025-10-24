import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { EntityManager } from "typeorm"
import { Booking } from "../../../models/booking"
import { S3Client, GetObjectCommand } from "@aws-sdk/client-s3"
import { getSignedUrl } from "@aws-sdk/s3-request-presigner"

// Configuração do S3
const s3Client = new S3Client({
  region: process.env.AWS_REGION || "us-east-2",
  credentials: {
    accessKeyId: process.env.AWS_ACCESS_KEY_ID!,
    secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY!,
  },
})

const BUCKET_NAME = process.env.AWS_S3_BUCKET_NAME || "meca-evidence-uploads"

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const bookingId = req.params.id
  const customerId = req.user?.customer_id

  if (!customerId) {
    res.status(401).json({ error: "Usuário não autenticado" })
    return
  }

  try {
    // Buscar agendamento
    const booking = await manager.findOne(Booking, {
      where: { id: bookingId },
      relations: ['customer']
    })

    if (!booking) {
      res.status(404).json({ error: "Agendamento não encontrado" })
      return
    }

    if (booking.customer.id !== customerId) {
      res.status(403).json({ error: "Sem permissão para este agendamento" })
      return
    }

    const evidenceUrls = booking.evidence_urls || []
    const signedUrls = []

    // Gerar URLs assinadas para cada evidência
    for (const evidenceUrl of evidenceUrls) {
      try {
        const key = evidenceUrl.replace(`s3://${BUCKET_NAME}/`, '')
        
        const command = new GetObjectCommand({
          Bucket: BUCKET_NAME,
          Key: key,
        })

        const signedUrl = await getSignedUrl(s3Client, command, {
          expiresIn: 300 // 5 minutos
        })

        signedUrls.push({
          originalUrl: evidenceUrl,
          signedUrl: signedUrl,
          fileName: key.split('/').pop()
        })
      } catch (error) {
        console.error(`Erro ao gerar URL assinada para ${evidenceUrl}:`, error)
      }
    }

    res.json({
      success: true,
      data: {
        evidence: signedUrls
      }
    })

  } catch (error) {
    console.error("Erro ao buscar evidências:", error)
    res.status(500).json({ error: "Erro interno do servidor" })
  }
}