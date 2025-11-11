import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { EntityManager } from "typeorm"
import { CardTokenizationService } from "../../services/card-tokenization-service"

export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const customerId = req.user?.customer_id

  if (!customerId) {
    res.status(401).json({ error: "Usuário não autenticado" })
    return
  }

  try {
    const result = await CardTokenizationService.getSavedCards(manager, customerId)
    
    if (result.success) {
      res.json({
        success: true,
        data: {
          cards: result.cards || []
        }
      })
    } else {
      res.status(400).json({ 
        success: false, 
        error: result.error 
      })
    }

  } catch (error) {
    console.error("Erro ao buscar cartões salvos:", error)
    res.status(500).json({ 
      success: false, 
      error: "Erro interno do servidor" 
    })
  }
}

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
): Promise<void> {
  const manager = req.scope.resolve<EntityManager>("manager")
  const customerId = req.user?.customer_id
  const { cardData, isDefault = false } = req.body

  if (!customerId) {
    res.status(401).json({ error: "Usuário não autenticado" })
    return
  }

  if (!cardData) {
    res.status(400).json({ error: "Dados do cartão são obrigatórios" })
    return
  }

  try {
    // Tokenizar cartão
    const tokenizationResult = await CardTokenizationService.tokenizeCard(cardData)
    
    if (!tokenizationResult.success) {
      res.status(400).json({ 
        success: false, 
        error: tokenizationResult.error 
      })
      return
    }

    // Salvar cartão tokenizado
    const saveResult = await CardTokenizationService.saveTokenizedCard(
      manager,
      customerId,
      tokenizationResult.token!,
      tokenizationResult.cardInfo!,
      isDefault
    )

    if (saveResult.success) {
      res.json({
        success: true,
        data: {
          cardId: saveResult.cardId,
          cardInfo: tokenizationResult.cardInfo
        }
      })
    } else {
      res.status(400).json({ 
        success: false, 
        error: saveResult.error 
      })
    }

  } catch (error) {
    console.error("Erro ao salvar cartão:", error)
    res.status(500).json({ 
      success: false, 
      error: "Erro interno do servidor" 
    })
  }
}












