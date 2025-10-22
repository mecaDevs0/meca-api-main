/**
 * Endpoints para oficinas
 * GET /api/customer/workshops - Listar oficinas próximas
 * GET /api/customer/workshops/[id] - Detalhes da oficina
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"

export const AUTHENTICATE = false

/**
 * GET /api/customer/workshops
 * Listar oficinas próximas
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const { lat, lng, radius = 10, status = 'aprovado' } = req.query

    const oficinaService = req.scope.resolve("oficinaModuleService")

    // Buscar oficinas aprovadas
    const oficinas = await oficinaService.list({
      status,
      take: 50
    })

    let filteredOficinas = oficinas

    // Se coordenadas fornecidas, filtrar por proximidade
    if (lat && lng) {
      const userLat = parseFloat(lat as string)
      const userLng = parseFloat(lng as string)
      const radiusKm = parseFloat(radius as string)

      filteredOficinas = oficinas.filter(oficina => {
        const metadata = oficina.metadata as any
        if (!metadata?.latitude || !metadata?.longitude) return false

        const oficinaLat = parseFloat(metadata.latitude)
        const oficinaLng = parseFloat(metadata.longitude)

        // Cálculo simples de distância (aproximado)
        const distance = Math.sqrt(
          Math.pow(userLat - oficinaLat, 2) + Math.pow(userLng - oficinaLng, 2)
        ) * 111 // Aproximação: 1 grau ≈ 111km

        return distance <= radiusKm
      })

      // Ordenar por proximidade
      filteredOficinas.sort((a, b) => {
        const aMetadata = a.metadata as any
        const bMetadata = b.metadata as any
        
        const distA = Math.sqrt(
          Math.pow(userLat - parseFloat(aMetadata?.latitude || 0), 2) + 
          Math.pow(userLng - parseFloat(aMetadata?.longitude || 0), 2)
        )
        
        const distB = Math.sqrt(
          Math.pow(userLat - parseFloat(bMetadata?.latitude || 0), 2) + 
          Math.pow(userLng - parseFloat(bMetadata?.longitude || 0), 2)
        )
        
        return distA - distB
      })
    }

    return res.json({
      workshops: filteredOficinas.map(oficina => ({
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        phone: oficina.phone,
        address: oficina.address,
        status: oficina.status,
        metadata: {
          company_name: oficina.metadata?.company_name,
          rating: oficina.metadata?.rating,
          latitude: oficina.metadata?.latitude,
          longitude: oficina.metadata?.longitude
        },
        created_at: oficina.created_at
      })),
      count: filteredOficinas.length
    })

  } catch (error) {
    console.error("Erro ao listar oficinas:", error)
    return res.status(500).json({
      message: "Erro ao listar oficinas",
      error: error.message
    })
  }
}
