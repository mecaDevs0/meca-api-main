import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"

export const AUTHENTICATE = false

/**
 * GET /admin/dashboard-metrics
 * 
 * Retorna métricas chave para o dashboard do administrador
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  // Dados mockados enquanto as tabelas não estão criadas
  const mockData = {
    total_customers: 248,
    total_orders: 156,
    total_revenue: 15420.50,
    active_workshops: 12,
    total_bookings: 127,
    confirmed_bookings: 98,
    conversion_rate: 77.2,
    bookings_by_day: [
      { date: '2024-10-11', count: 8 },
      { date: '2024-10-12', count: 12 },
      { date: '2024-10-13', count: 15 },
      { date: '2024-10-14', count: 9 },
      { date: '2024-10-15', count: 18 },
      { date: '2024-10-16', count: 22 },
      { date: '2024-10-17', count: 16 }
    ],
    revenue_by_day: [
      { date: '2024-10-11', amount: 850.00 },
      { date: '2024-10-12', amount: 1200.50 },
      { date: '2024-10-13', amount: 1850.75 },
      { date: '2024-10-14', amount: 920.25 },
      { date: '2024-10-15', amount: 2100.00 },
      { date: '2024-10-16', amount: 2650.50 },
      { date: '2024-10-17', amount: 1847.50 }
    ]
  }

  try {
    return res.json(mockData)
  } catch (error) {
    console.error("Erro ao buscar métricas do dashboard:", error)
    return res.status(500).json({
      message: "Erro interno ao buscar métricas",
      error: error.message
    })
  }
}