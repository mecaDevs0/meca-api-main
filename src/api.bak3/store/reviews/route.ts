import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { REVIEW_MODULE } from "../../../modules/review"
import { BOOKING_MODULE } from "../../../modules/booking"
import { BookingStatus } from "../../../modules/booking/models/booking"

/**
 * GET /store/reviews?oficina_id={id}
 * 
 * Lista avaliações de uma oficina
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const reviewModuleService = req.scope.resolve(REVIEW_MODULE)
  
  const { oficina_id, limit = 20, offset = 0 } = req.query
  
  if (!oficina_id) {
    return res.status(400).json({
      message: "oficina_id é obrigatório"
    })
  }
  
  try {
    const reviews = await reviewModuleService.listReviews({
      oficina_id,
      is_approved: true,
    }, {
      take: Number(limit),
      skip: Number(offset),
      order: { created_at: "DESC" }
    })
    
    // Calcular média de avaliações
    const avgRating = reviews.length > 0
      ? reviews.reduce((sum, r) => sum + r.rating, 0) / reviews.length
      : 0
    
    return res.json({
      reviews,
      count: reviews.length,
      average_rating: Number(avgRating.toFixed(1)),
    })
    
  } catch (error) {
    console.error("Erro ao listar avaliações:", error)
    
    return res.status(500).json({
      message: "Erro ao listar avaliações",
      error: error.message
    })
  }
}

/**
 * POST /store/reviews
 * 
 * Cliente cria uma avaliação após serviço finalizado
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const reviewModuleService = req.scope.resolve(REVIEW_MODULE)
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  
  const customerId = req.auth_context?.actor_id
  
  if (!customerId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  const {
    booking_id,
    rating,
    title,
    comment
  } = req.body
  
  // Validações
  if (!booking_id || !rating) {
    return res.status(400).json({
      message: "booking_id e rating são obrigatórios"
    })
  }
  
  if (rating < 1 || rating > 5) {
    return res.status(400).json({
      message: "Rating deve ser entre 1 e 5"
    })
  }
  
  try {
    // Buscar booking
    const booking = await bookingModuleService.retrieveBooking(booking_id)
    
    // Verificar se booking pertence ao cliente
    if (booking.customer_id !== customerId) {
      return res.status(403).json({ message: "Acesso negado" })
    }
    
    // Verificar se serviço foi finalizado
    if (booking.status !== BookingStatus.FINALIZADO_CLIENTE && 
        booking.status !== BookingStatus.FINALIZADO_MECANICO) {
      return res.status(400).json({
        message: "Só é possível avaliar serviços finalizados"
      })
    }
    
    // Verificar se já existe avaliação para este booking
    const existingReview = await reviewModuleService.listReviews({ booking_id })
    
    if (existingReview.length > 0) {
      return res.status(400).json({
        message: "Você já avaliou este serviço"
      })
    }
    
    // Criar avaliação
    const review = await reviewModuleService.createReviews({
      customer_id: customerId,
      oficina_id: booking.oficina_id,
      booking_id,
      product_id: booking.product_id,
      rating,
      title,
      comment,
      is_approved: true, // Auto-aprovar por padrão
    })
    
    return res.status(201).json({
      message: "Avaliação criada com sucesso",
      review
    })
    
  } catch (error) {
    console.error("Erro ao criar avaliação:", error)
    
    return res.status(500).json({
      message: "Erro ao criar avaliação",
      error: error.message
    })
  }
}

