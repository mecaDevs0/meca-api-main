import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { OFICINA_MODULE } from "../../../../../modules/oficina"

/**
 * GET /store/workshops/me/schedule
 * 
 * Busca a agenda semanal da oficina
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
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
    
    return res.json({
      success: true,
      data: {
        horario_funcionamento: oficina.horario_funcionamento || {},
        is_configured: oficina.horario_funcionamento != null
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar agenda:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar agenda",
      error: error.message
    })
  }
}

/**
 * PUT /store/workshops/me/schedule
 * 
 * Atualiza a agenda semanal da oficina
 */
export async function PUT(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const oficinaModuleService = req.scope.resolve(OFICINA_MODULE)
  
  const userId = req.auth_context?.actor_id
  const { schedule } = req.body
  
  if (!userId) {
    return res.status(401).json({ message: "Não autenticado" })
  }
  
  if (!schedule) {
    return res.status(400).json({
      message: "schedule é obrigatório"
    })
  }
  
  try {
    // Buscar oficina do usuário
    const oficinas = await oficinaModuleService.listOficinas({}, { take: 1 })
    
    if (!oficinas || oficinas.length === 0) {
      return res.status(404).json({ message: "Oficina não encontrada" })
    }
    
    const oficinaId = oficinas[0].id
    
    // Validar estrutura da agenda
    const validDays = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday']
    const validSchedule = {}
    
    for (const day of validDays) {
      if (schedule[day]) {
        const daySchedule = schedule[day]
        if (daySchedule.is_open && daySchedule.start_time && daySchedule.end_time) {
          validSchedule[day] = {
            is_open: daySchedule.is_open,
            start_time: daySchedule.start_time,
            end_time: daySchedule.end_time,
            break_start: daySchedule.break_start || null,
            break_end: daySchedule.break_end || null,
          }
        } else {
          validSchedule[day] = {
            is_open: false,
            start_time: null,
            end_time: null,
            break_start: null,
            break_end: null,
          }
        }
      } else {
        validSchedule[day] = {
          is_open: false,
          start_time: null,
          end_time: null,
          break_start: null,
          break_end: null,
        }
      }
    }
    
    // Atualizar oficina
    const updatedOficina = await oficinaModuleService.updateOficinas([{
      id: oficinaId,
      horario_funcionamento: validSchedule
    }])
    
    return res.json({
      success: true,
      message: "Agenda atualizada com sucesso",
      data: {
        horario_funcionamento: validSchedule
      }
    })
    
  } catch (error) {
    console.error("Erro ao atualizar agenda:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao atualizar agenda",
      error: error.message
    })
  }
}





