import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../../modules/booking"
import { OFICINA_MODULE } from "../../../../modules/oficina"
import { AuthService } from "../../../../services/auth"

export const AUTHENTICATE = false

/**
 * GET /public/workshops/dashboard
 * 
 * Dashboard da oficina com dados REAIS do banco de dados
 * Retorna métricas e agendamentos
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaService = req.scope.resolve(OFICINA_MODULE)
  const bookingService = req.scope.resolve(BOOKING_MODULE)
  const productService = req.scope.resolve(Modules.PRODUCT)

  // Extrair e verificar token JWT
  const token = AuthService.extractTokenFromHeader(req.headers.authorization)
  
  if (!token) {
    return res.status(401).json({
      message: "Token de autenticação não fornecido"
    })
  }

  try {
    const decoded = AuthService.verifyToken(token)
    const oficinaId = decoded.id

    // Buscar oficina REAL do banco
    const oficina = await oficinaService.retrieveOficina(oficinaId)

    if (!oficina) {
      return res.status(404).json({
        message: "Oficina não encontrada"
      })
    }

    // Buscar agendamentos REAIS da oficina
    const agendamentos = await bookingService.listBookings({
      oficina_id: oficinaId
    }, {
      order: { created_at: "DESC" },
      take: 50
    })

    // Buscar serviços REAIS da oficina
    const servicos = await productService.listProducts({
      metadata: {
        oficina_id: oficinaId
      }
    })

    // Calcular métricas REAIS
    const totalAgendamentos = agendamentos.length
    const agendamentosPendentes = agendamentos.filter(a => a.status === 'pendente').length
    const agendamentosConfirmados = agendamentos.filter(a => a.status === 'confirmado').length
    const agendamentosConcluidos = agendamentos.filter(a => a.status === 'concluido').length
    const agendamentosCancelados = agendamentos.filter(a => a.status === 'cancelado').length

    // Calcular receita do mês (agendamentos concluídos)
    const now = new Date()
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1)
    const agendamentosDoMes = agendamentos.filter(a => 
      a.status === 'concluido' && 
      new Date(a.created_at) >= startOfMonth
    )

    return res.json({
      oficina: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        cnpj: oficina.cnpj,
        status: oficina.status,
        phone: oficina.phone,
        address: oficina.address,
        description: oficina.description,
        horario_funcionamento: oficina.horario_funcionamento,
        logo_url: oficina.logo_url,
        photo_urls: oficina.photo_urls,
        dados_bancarios: oficina.dados_bancarios
      },
      metricas: {
        total_agendamentos: totalAgendamentos,
        agendamentos_pendentes: agendamentosPendentes,
        agendamentos_confirmados: agendamentosConfirmados,
        agendamentos_concluidos: agendamentosConcluidos,
        agendamentos_cancelados: agendamentosCancelados,
        total_servicos: servicos.length,
        agendamentos_mes: agendamentosDoMes.length
      },
      agendamentos_recentes: agendamentos.slice(0, 10).map(a => ({
        id: a.id,
        customer_id: a.customer_id,
        vehicle_id: a.vehicle_id,
        service_id: a.service_id,
        appointment_date: a.appointment_date,
        appointment_time: a.appointment_time,
        status: a.status,
        customer_notes: a.customer_notes,
        created_at: a.created_at
      })),
      servicos: servicos.map(s => ({
        id: s.id,
        title: s.title,
        description: s.description,
        metadata: s.metadata
      }))
    })

  } catch (error) {
    console.error("Erro ao buscar dashboard:", error)

    if (error.message === 'Invalid or expired token') {
      return res.status(401).json({
        message: "Token inválido ou expirado"
      })
    }

    return res.status(500).json({
      message: "Erro ao buscar dados do dashboard",
      error: error.message
    })
  }
}

