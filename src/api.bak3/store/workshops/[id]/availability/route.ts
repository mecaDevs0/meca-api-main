import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

export const AUTHENTICATE = false

/**
 * GET /store/workshops/[id]/availability
 *
 * Buscar horários disponíveis de uma oficina para uma data específica
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  const bookingModuleService = req.scope.resolve(Modules.BOOKING)
  
  const { id } = req.params
  const { date } = req.query

  if (!date) {
    return res.status(400).json({
      message: "Data é obrigatória"
    })
  }

  try {
    // Verificar se a oficina existe e está aprovada
    const workshop = await oficinaModuleService.retrieveOficina(id)
    if (!workshop || workshop.status !== "aprovado") {
      return res.status(404).json({
        message: "Oficina não encontrada ou não aprovada"
      })
    }

    // Buscar agendamentos existentes para a data
    const existingBookings = await bookingModuleService.listBookings({
      workshop_id: id,
      scheduled_date: date as string,
      status: ["pendente", "confirmado", "em_andamento"]
    })

    // Horários disponíveis (8h às 18h, intervalos de 1 hora)
    const availableHours = []
    const workingHours = workshop.horario_funcionamento || {
      segunda: { inicio: "08:00", fim: "18:00" },
      terca: { inicio: "08:00", fim: "18:00" },
      quarta: { inicio: "08:00", fim: "18:00" },
      quinta: { inicio: "08:00", fim: "18:00" },
      sexta: { inicio: "08:00", fim: "18:00" },
      sabado: { inicio: "08:00", fim: "12:00" },
      domingo: { inicio: "00:00", fim: "00:00" }
    }

    // Obter dia da semana
    const dateObj = new Date(date as string)
    const dayOfWeek = dateObj.getDay()
    const dayNames = ["domingo", "segunda", "terca", "quarta", "quinta", "sexta", "sabado"]
    const dayName = dayNames[dayOfWeek]

    const daySchedule = workingHours[dayName]
    if (!daySchedule || daySchedule.inicio === "00:00") {
      return res.json({
        date: date,
        available_hours: [],
        message: "Oficina não funciona neste dia"
      })
    }

    // Gerar horários disponíveis
    const startTime = parseInt(daySchedule.inicio.split(":")[0])
    const endTime = parseInt(daySchedule.fim.split(":")[0])
    
    for (let hour = startTime; hour < endTime; hour++) {
      const timeString = `${hour.toString().padStart(2, "0")}:00`
      
      // Verificar se há agendamento neste horário
      const hasBooking = existingBookings.some(booking => 
        booking.scheduled_time === timeString
      )
      
      if (!hasBooking) {
        availableHours.push(timeString)
      }
    }

    return res.json({
      date: date,
      available_hours: availableHours,
      total_available: availableHours.length,
      workshop_schedule: daySchedule
    })

  } catch (error) {
    console.error("Erro ao buscar disponibilidade:", error)
    return res.status(500).json({
      message: "Erro ao buscar disponibilidade",
      error: error.message
    })
  }
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


