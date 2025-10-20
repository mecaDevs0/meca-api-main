import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"

/**
 * POST /store/vehicles/lookup-by-plate
 * 
 * Consulta dados de veículo por placa
 * Mock implementado - pronto para integração com API real
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { plate } = req.body
  
  if (!plate) {
    return res.status(400).json({
      message: "Placa é obrigatória"
    })
  }
  
  // Validação básica de formato de placa brasileira
  const plateRegex = /^[A-Z]{3}[0-9]{4}$|^[A-Z]{3}[0-9][A-Z][0-9]{2}$/
  
  if (!plateRegex.test(plate.replace(/[^A-Z0-9]/g, '').toUpperCase())) {
    return res.status(400).json({
      message: "Formato de placa inválido"
    })
  }
  
  try {
    // MOCK: Dados fictícios baseados na placa
    // Em produção, aqui seria feita a consulta à API do Detran/Denatran
    
    const mockData = generateMockVehicleData(plate)
    
    return res.json({
      success: true,
      data: mockData,
      source: "mock", // Indica que são dados fictícios
      message: "Dados encontrados (mock - para desenvolvimento)"
    })
    
  } catch (error) {
    console.error("Erro ao consultar placa:", error)
    
    return res.status(500).json({
      message: "Erro interno ao consultar placa",
      error: error.message
    })
  }
}

/**
 * Gera dados fictícios baseados na placa
 * Simula resposta de API real de consulta veicular
 */
function generateMockVehicleData(plate: string): any {
  const brands = ["Fiat", "Volkswagen", "Chevrolet", "Ford", "Toyota", "Honda", "Hyundai", "Nissan"]
  const models = {
    "Fiat": ["Uno", "Palio", "Siena", "Strada", "Toro"],
    "Volkswagen": ["Gol", "Polo", "Jetta", "Passat", "Amarok"],
    "Chevrolet": ["Celta", "Corsa", "Prisma", "Onix", "S10"],
    "Ford": ["Ka", "Fiesta", "Focus", "EcoSport", "Ranger"],
    "Toyota": ["Corolla", "Hilux", "Etios", "Yaris", "SW4"],
    "Honda": ["Civic", "Fit", "City", "HR-V", "CR-V"],
    "Hyundai": ["HB20", "HB20S", "Elantra", "Tucson", "Creta"],
    "Nissan": ["March", "Versa", "Sentra", "Kicks", "Frontier"]
  }
  
  const colors = ["Branco", "Prata", "Preto", "Vermelho", "Azul", "Verde", "Amarelo", "Cinza"]
  const fuelTypes = ["Flex", "Gasolina", "Etanol", "Diesel", "GNV"]
  
  // Usar placa como seed para gerar dados consistentes
  const seed = plate.charCodeAt(0) + plate.charCodeAt(1) + plate.charCodeAt(2)
  const brandIndex = seed % brands.length
  const brand = brands[brandIndex]
  const model = models[brand][seed % models[brand].length]
  const year = 2010 + (seed % 14) // 2010-2023
  const color = colors[seed % colors.length]
  const fuel = fuelTypes[seed % fuelTypes.length]
  
  return {
    plate: plate.toUpperCase(),
    brand,
    model,
    year,
    color,
    fuel_type: fuel,
    chassis: `9BWZZZZ${plate.replace(/[^A-Z0-9]/g, '')}${Math.random().toString(36).substr(2, 5).toUpperCase()}`,
    engine: `${Math.floor(Math.random() * 2) + 1}.${Math.floor(Math.random() * 9) + 1}`,
    doors: Math.floor(Math.random() * 2) + 3, // 3 ou 4 portas
    transmission: Math.random() > 0.5 ? "Manual" : "Automático",
    status: "Ativo",
    last_inspection: new Date(Date.now() - Math.random() * 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    insurance_expiry: new Date(Date.now() + Math.random() * 365 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
  }
}
