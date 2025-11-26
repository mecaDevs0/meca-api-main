import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { EntityManager } from "typeorm"
import { CardTokenizationService } from "../../../../services/card-tokenization-service"

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const customerId = req.user?.customer_id
  const cardId = req.params.id

  if (!customerId) {
    res.status(401).json({ error: "Usuário não autenticado" })
    return
  }

  try {
    const result = await CardTokenizationService.setDefaultCard(
      manager,
      customerId,
      cardId
    )

    if (result.success) {
      res.json({
        success: true,
        message: "Cartão definido como padrão"
      })
    } else {
      res.status(400).json({ 
        success: false, 
        error: result.error 
      })
    }

  } catch (error) {
    console.error("Erro ao definir cartão padrão:", error)
    res.status(500).json({ 
      success: false, 
      error: "Erro interno do servidor" 
    })
  }
}





















