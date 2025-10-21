import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../../../../../modules/booking"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * GET /store/workshops/me/dashboard
 * 
 * Dashboard da oficina com métricas e dados resumidos
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  
  const userId = req.auth_context?.actor_id
  
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
    const oficinaId = oficina.id
    
    // Buscar agendamentos da oficina
    const bookings = await bookingModuleService.listBookings({
      oficina_id: oficinaId
    })
    
    // Calcular métricas
    const totalBookings = bookings.length
    const pendingBookings = bookings.filter(b => b.status === 'pendente_oficina').length
    const confirmedBookings = bookings.filter(b => b.status === 'confirmado').length
    const completedBookings = bookings.filter(b => b.status === 'concluido').length
    const cancelledBookings = bookings.filter(b => b.status === 'cancelado' || b.status === 'recusado').length
    
    // Calcular faturamento
    const totalRevenue = bookings
      .filter(b => b.status === 'concluido')
      .reduce((sum, b) => sum + (b.estimated_price || 0), 0)
    
    // Buscar serviços da oficina
    const services = await productModuleService.listProducts({
      metadata: {
        oficina_id: oficinaId
      }
    })
    
    // Verificar configurações pendentes
    const hasBankAccount = oficina.metadata?.bank_account != null
    const hasSchedule = oficina.horario_funcionamento != null
    const hasServices = services.length > 0
    
    // Próximos agendamentos (próximos 7 dias)
    const now = new Date()
    const nextWeek = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000)
    
    const upcomingBookings = bookings
      .filter(b => {
        const bookingDate = new Date(b.appointment_date)
        return bookingDate >= now && bookingDate <= nextWeek && 
               (b.status === 'pendente_oficina' || b.status === 'confirmado')
      })
      .sort((a, b) => new Date(a.appointment_date).getTime() - new Date(b.appointment_date).getTime())
      .slice(0, 5)
    
    return res.json({
      success: true,
      data: {
        workshop: {
          id: oficina.id,
          name: oficina.name,
          status: oficina.status,
          email: oficina.email,
          phone: oficina.phone,
          address: oficina.address,
          description: oficina.description,
          logo_url: oficina.logo_url,
          photo_urls: oficina.photo_urls,
          horario_funcionamento: oficina.horario_funcionamento,
          bank_account: oficina.metadata?.bank_account,
          schedule: oficina.horario_funcionamento,
          services: services,
        },
        metrics: {
          total_bookings: totalBookings,
          pending_bookings: pendingBookings,
          confirmed_bookings: confirmedBookings,
          completed_bookings: completedBookings,
          cancelled_bookings: cancelledBookings,
          total_revenue: totalRevenue,
          services_count: services.length,
        },
        setup_status: {
          bank_account_valid: hasBankAccount,
          schedule_valid: hasSchedule,
          services_valid: hasServices,
          is_complete: hasBankAccount && hasSchedule && hasServices,
        },
        upcoming_bookings: upcomingBookings,
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar dashboard:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar dados do dashboard",
      error: error.message
    })
  }
}





















