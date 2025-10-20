import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"
import { OFICINA_MODULE } from "../../../modules/oficina"

export const AUTHENTICATE = false

/**
 * GET /admin/users
 * 
 * Lista todos os usuários (customers e workshops)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const customerService = req.scope.resolve(Modules.CUSTOMER)
  const oficinaService = req.scope.resolve(OFICINA_MODULE)
  const userService = req.scope.resolve(Modules.USER)
  
  const { type, limit = 100, offset = 0 } = req.query
  
  try {
    let users: any[] = []
    
    if (!type || type === 'customer') {
      const customers = await customerService.listCustomers({}, {
        take: Number(limit),
        skip: Number(offset),
        select: ['id', 'email', 'first_name', 'last_name', 'phone', 'created_at']
      })
      
      users = [
        ...users,
        ...customers.map(c => ({
          id: c.id,
          name: `${c.first_name} ${c.last_name}`,
          email: c.email,
          phone: c.phone || '',
          type: 'customer',
          created_at: c.created_at
        }))
      ]
    }
    
    if (!type || type === 'workshop') {
      const oficinas = await oficinaService.listOficinas({}, {
        take: Number(limit),
        skip: Number(offset)
      })
      
      users = [
        ...users,
        ...oficinas.map(o => ({
          id: o.id,
          name: o.name,
          email: o.email,
          phone: o.phone || '',
          type: 'workshop',
          created_at: o.created_at
        }))
      ]
    }
    
    return res.json({
      users,
      count: users.length
    })
    
  } catch (error) {
    console.error("Erro ao listar usuários:", error)
    
    return res.status(500).json({
      message: "Erro ao listar usuários",
      error: error.message
    })
  }
}
