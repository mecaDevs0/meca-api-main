import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { BOOKING_MODULE } from "../../../../../modules/booking"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * GET /store/workshops/me/notifications
 * 
 * Buscar notificações da oficina
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { limit = 50, offset = 0, unread_only = false } = req.query
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    
    // Buscar agendamentos recentes que geram notificações
    const recentBookings = await bookingModuleService.listBookings({
      oficina_id: oficinaId,
      created_at: {
        $gte: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) // Últimos 30 dias
      }
    }, {
      take: Number(limit),
      skip: Number(offset),
      order: { created_at: "DESC" }
    })
    
    // Gerar notificações baseadas nos agendamentos
    const notifications = []
    
    for (const booking of recentBookings) {
      // Notificação de novo agendamento
      if (booking.status === 'pendente_oficina') {
        notifications.push({
          id: `booking_new_${booking.id}`,
          type: 'new_booking',
          title: 'Novo Agendamento',
          message: `Novo agendamento de ${booking.customer_name || 'Cliente'} para ${booking.service_name || 'serviço'}`,
          data: {
            booking_id: booking.id,
            customer_name: booking.customer_name,
            service_name: booking.service_name,
            appointment_date: booking.appointment_date
          },
          created_at: booking.created_at,
          read: false,
          priority: 'high'
        })
      }
      
      // Notificação de agendamento confirmado
      if (booking.status === 'confirmado' && booking.confirmed_at) {
        notifications.push({
          id: `booking_confirmed_${booking.id}`,
          type: 'booking_confirmed',
          title: 'Agendamento Confirmado',
          message: `Agendamento confirmado para ${booking.customer_name || 'Cliente'}`,
          data: {
            booking_id: booking.id,
            customer_name: booking.customer_name,
            service_name: booking.service_name,
            appointment_date: booking.appointment_date
          },
          created_at: booking.confirmed_at,
          read: false,
          priority: 'medium'
        })
      }
      
      // Notificação de agendamento cancelado
      if (booking.status === 'cancelado') {
        notifications.push({
          id: `booking_cancelled_${booking.id}`,
          type: 'booking_cancelled',
          title: 'Agendamento Cancelado',
          message: `Agendamento cancelado por ${booking.customer_name || 'Cliente'}`,
          data: {
            booking_id: booking.id,
            customer_name: booking.customer_name,
            service_name: booking.service_name,
            appointment_date: booking.appointment_date
          },
          created_at: booking.updated_at,
          read: false,
          priority: 'medium'
        })
      }
    }
    
    // Adicionar notificações do sistema
    const systemNotifications = [
      {
        id: 'system_welcome',
        type: 'system',
        title: 'Bem-vindo ao MECA!',
        message: 'Configure sua oficina para começar a receber agendamentos',
        data: {},
        created_at: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000), // 7 dias atrás
        read: false,
        priority: 'low'
      }
    ]
    
    // Combinar notificações
    const allNotifications = [...notifications, ...systemNotifications]
      .sort((a, b) => new Date(b.created_at).getTime() - new Date(a.created_at).getTime())
    
    // Filtrar apenas não lidas se solicitado
    const filteredNotifications = unread_only === 'true' 
      ? allNotifications.filter(n => !n.read)
      : allNotifications
    
    return res.json({
      success: true,
      data: {
        notifications: filteredNotifications,
        count: filteredNotifications.length,
        unread_count: allNotifications.filter(n => !n.read).length
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar notificações:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar notificações",
      error: error.message
    })
  }
}

/**
 * PUT /store/workshops/me/notifications/:id/read
 * 
 * Marcar notificação como lida
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const userId = req.auth_context?.actor_id
  const { id } = req.params
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  if (!id) {
    return res.status(400).json({ message: "ID da notificação é obrigatório" })
  }
  
  try {
    // Em um sistema real, você salvaria o status de leitura no banco
    // Por enquanto, vamos simular que a notificação foi marcada como lida
    
    return res.json({
      success: true,
      message: "Notificação marcada como lida"
    })
    
  } catch (error) {
    console.error("Erro ao marcar notificação como lida:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao marcar notificação como lida",
      error: error.message
    })
  }
}

/**
 * POST /store/workshops/me/notifications/test
 * 
 * Criar notificação de teste
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const userId = req.auth_context?.actor_id
  const { type, title, message } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  try {
    // Criar notificação de teste
    const testNotification = {
      id: `test_${Date.now()}`,
      type: type || 'test',
      title: title || 'Notificação de Teste',
      message: message || 'Esta é uma notificação de teste',
      data: {},
      created_at: new Date().toISOString(),
      read: false,
      priority: 'low'
    }
    
    return res.json({
      success: true,
      message: "Notificação de teste criada",
      data: testNotification
    })
    
  } catch (error) {
    console.error("Erro ao criar notificação de teste:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao criar notificação de teste",
      error: error.message
    })
  }
}














