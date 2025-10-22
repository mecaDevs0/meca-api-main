/**
 * Endpoint para aprovar oficinas
 * POST /admin/workshops/[id]/approve
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import EmailService from "../../../services/email"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { id } = req.params

  if (!id) {
    return res.status(400).json({
      message: "ID da oficina é obrigatório"
    })
  }

  try {
    const oficinaService = req.scope.resolve("oficinaModuleService")
    
    // Buscar oficina
    const oficinas = await oficinaService.list({
      id,
      take: 1
    })

    if (oficinas.length === 0) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }

    const oficina = oficinas[0]

    if (oficina.status === 'aprovado') {
      return res.status(400).json({
        message: "Oficina já está aprovada"
      })
    }

    // Aprovar oficina
    await oficinaService.update(id, {
      status: 'aprovado',
      metadata: {
        ...oficina.metadata,
        approved_at: new Date().toISOString(),
        approved_by: req.user?.id || 'admin'
      }
    })

    // Enviar email de aprovação
    try {
      await EmailService.sendWorkshopApproved({
        name: oficina.name,
        email: oficina.email,
        companyName: oficina.metadata?.company_name || oficina.name
      })
    } catch (emailError) {
      console.error("Erro ao enviar email de aprovação:", emailError)
      // Não falhar a aprovação por erro de email
    }

    return res.json({
      message: "Oficina aprovada com sucesso!",
      workshop: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        status: 'aprovado'
      }
    })

  } catch (error) {
    console.error("Erro ao aprovar oficina:", error)

    return res.status(500).json({
      message: "Erro ao aprovar oficina",
      error: error.message
    })
  }
}
