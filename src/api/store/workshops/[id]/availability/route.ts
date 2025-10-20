import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"
import { BOOKING_MODULE } from "../../../../../modules/booking"

/**
 * GET /store/workshops/:id/availability?date=2025-10-20
 * 
 * Retorna horários disponíveis para agendamento em uma data específica
 * 
 * Query params:
 * - date: Data no formato YYYY-MM-DD
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const bookingModuleService = req.scope.resolve(BOOKING_MODULE)
  
  const { id: oficinaId } = req.params
  const { date } = req.query
  
  if (!date) {
    return res.status(400).json({
      message: "Parâmetro 'date' é obrigatório (formato: YYYY-MM-DD)"
    })
  }
  
  try {
    // Buscar oficina
    const oficina = await oficinaModuleService.retrieveOficina(oficinaId)
    
    if (!oficina || oficina.status !== "aprovado") {
      return res.status(404).json({
        message: "Oficina não encontrada ou não está disponível"
      })
    }
    
    // Verificar se oficina configurou horário de funcionamento
    if (!oficina.horario_funcionamento) {
      return res.status(400).json({
        message: "Oficina ainda não configurou horário de funcionamento"
      })
    }
    
    // Obter dia da semana da data solicitada
    const targetDate = new Date(date as string)
    const dayOfWeek = targetDate.toLocaleDateString('pt-BR', { weekday: 'short' }).toLowerCase()
    
    // Mapear dias da semana
    const dayMap = {
      'dom': 'dom',
      'seg': 'seg',
      'ter': 'ter',
      'qua': 'qua',
      'qui': 'qui',
      'sex': 'sex',
      'sáb': 'sab'
    }
    
    const dayKey = dayMap[dayOfWeek] || dayOfWeek
    
    // Verificar se oficina trabalha neste dia
    const horarioDoDia = oficina.horario_funcionamento[dayKey]
    
    if (!horarioDoDia || horarioDoDia === null) {
      return res.json({
        available_slots: [],
        message: "Oficina não funciona neste dia"
      })
    }
    
    // Gerar slots de horário (a cada 30 minutos)
    const { inicio, fim, pause_inicio, pause_fim } = horarioDoDia
    const slots: string[] = []
    
    let currentTime = inicio // ex: "08:00"
    const endTime = fim // ex: "18:00"
    
    while (currentTime < endTime) {
      // Verificar se não está no horário de pausa
      if (pause_inicio && pause_fim) {
        if (currentTime >= pause_inicio && currentTime < pause_fim) {
          currentTime = addMinutes(currentTime, 30)
          continue
        }
      }
      
      slots.push(currentTime)
      currentTime = addMinutes(currentTime, 30)
    }
    
    // Buscar agendamentos já existentes para este dia
    const startOfDay = new Date(targetDate)
    startOfDay.setHours(0, 0, 0, 0)
    
    const endOfDay = new Date(targetDate)
    endOfDay.setHours(23, 59, 59, 999)
    
    const bookings = await bookingModuleService.listBookings({
      oficina_id: oficinaId,
      appointment_date: {
        $gte: startOfDay,
        $lte: endOfDay
      },
      status: {
        $in: ["pendente_oficina", "confirmado"]
      }
    })
    
    // Remover horários já agendados
    const bookedSlots = bookings.map(b => {
      const time = new Date(b.appointment_date)
      return `${time.getHours().toString().padStart(2, '0')}:${time.getMinutes().toString().padStart(2, '0')}`
    })
    
    const availableSlots = slots.filter(slot => !bookedSlots.includes(slot))
    
    return res.json({
      date: date,
      day_of_week: dayKey,
      working_hours: horarioDoDia,
      available_slots: availableSlots,
      total_slots: slots.length,
      booked_slots: bookedSlots.length,
    })
    
  } catch (error) {
    console.error("Erro ao verificar disponibilidade:", error)
    
    return res.status(500).json({
      message: "Erro ao verificar disponibilidade",
      error: error.message
    })
  }
}

// Helper para adicionar minutos a um horário "HH:MM"
function addMinutes(time: string, minutes: number): string {
  const [hours, mins] = time.split(':').map(Number)
  const totalMinutes = hours * 60 + mins + minutes
  const newHours = Math.floor(totalMinutes / 60)
  const newMins = totalMinutes % 60
  
  return `${newHours.toString().padStart(2, '0')}:${newMins.toString().padStart(2, '0')}`
}

