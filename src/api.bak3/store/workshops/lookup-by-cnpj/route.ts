import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"

/**
 * POST /store/workshops/lookup-by-cnpj
 * 
 * Consulta dados de empresa por CNPJ
 * Mock implementado - pronto para integração com Serasa/Receita Federal
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { cnpj } = req.body
  
  if (!cnpj) {
    return res.status(400).json({
      message: "CNPJ é obrigatório"
    })
  }
  
  // Validação básica de CNPJ
  const cleanCnpj = cnpj.replace(/[^\d]/g, '')
  
  if (cleanCnpj.length !== 14) {
    return res.status(400).json({
      message: "CNPJ deve ter 14 dígitos"
    })
  }
  
  if (!isValidCNPJ(cleanCnpj)) {
    return res.status(400).json({
      message: "CNPJ inválido"
    })
  }
  
  try {
    // MOCK: Dados fictícios baseados no CNPJ
    // Em produção, aqui seria feita a consulta à API da Receita Federal/Serasa
    
    const mockData = generateMockCompanyData(cleanCnpj)
    
    return res.json({
      success: true,
      data: mockData,
      source: "mock", // Indica que são dados fictícios
      message: "Dados encontrados (mock - para desenvolvimento)"
    })
    
  } catch (error) {
    console.error("Erro ao consultar CNPJ:", error)
    
    return res.status(500).json({
      message: "Erro interno ao consultar CNPJ",
      error: error.message
    })
  }
}

/**
 * Valida CNPJ usando algoritmo oficial
 */
function isValidCNPJ(cnpj: string): boolean {
  // Remove caracteres não numéricos
  cnpj = cnpj.replace(/[^\d]/g, '')
  
  // Verifica se tem 14 dígitos
  if (cnpj.length !== 14) return false
  
  // Verifica se todos os dígitos são iguais
  if (/^(\d)\1+$/.test(cnpj)) return false
  
  // Validação do primeiro dígito verificador
  let sum = 0
  let weight = 5
  
  for (let i = 0; i < 12; i++) {
    sum += parseInt(cnpj[i]) * weight
    weight = weight === 2 ? 9 : weight - 1
  }
  
  let digit1 = sum % 11 < 2 ? 0 : 11 - (sum % 11)
  
  if (parseInt(cnpj[12]) !== digit1) return false
  
  // Validação do segundo dígito verificador
  sum = 0
  weight = 6
  
  for (let i = 0; i < 13; i++) {
    sum += parseInt(cnpj[i]) * weight
    weight = weight === 2 ? 9 : weight - 1
  }
  
  let digit2 = sum % 11 < 2 ? 0 : 11 - (sum % 11)
  
  return parseInt(cnpj[13]) === digit2
}

/**
 * Gera dados fictícios baseados no CNPJ
 * Simula resposta de API real de consulta empresarial
 */
function generateMockCompanyData(cnpj: string): any {
  const companyTypes = [
    "Oficina Mecânica",
    "Auto Center",
    "Centro Automotivo",
    "Serviços Automotivos",
    "Mecânica Especializada",
    "Auto Peças e Serviços"
  ]
  
  const cities = [
    { name: "São Paulo", state: "SP", zip: "01000-000" },
    { name: "Rio de Janeiro", state: "RJ", zip: "20000-000" },
    { name: "Belo Horizonte", state: "MG", zip: "30000-000" },
    { name: "Salvador", state: "BA", zip: "40000-000" },
    { name: "Brasília", state: "DF", zip: "70000-000" },
    { name: "Fortaleza", state: "CE", zip: "60000-000" },
    { name: "Manaus", state: "AM", zip: "69000-000" },
    { name: "Curitiba", state: "PR", zip: "80000-000" }
  ]
  
  const streets = [
    "Rua das Flores", "Avenida Central", "Rua do Comércio", "Avenida Principal",
    "Rua da Paz", "Avenida Brasil", "Rua São João", "Avenida Paulista"
  ]
  
  // Usar CNPJ como seed para gerar dados consistentes
  const seed = parseInt(cnpj.substr(0, 8))
  const companyType = companyTypes[seed % companyTypes.length]
  const city = cities[seed % cities.length]
  const street = streets[seed % streets.length]
  const number = (seed % 9999) + 1
  
  // Gerar razão social baseada no tipo
  const companyNames = {
    "Oficina Mecânica": ["Auto Mecânica Silva", "Oficina do João", "Mecânica Central", "Auto Serviços"],
    "Auto Center": ["Auto Center Premium", "Centro Automotivo Total", "Auto Center Express", "Centro do Carro"],
    "Centro Automotivo": ["Centro Auto Plus", "Automotivo Central", "Centro Car Service", "Auto Centro"],
    "Serviços Automotivos": ["Auto Serviços Pro", "Serviços do Carro", "Auto Manutenção", "Car Service"],
    "Mecânica Especializada": ["Mecânica Especial", "Auto Especializada", "Mecânica Pro", "Especial Auto"],
    "Auto Peças e Serviços": ["Auto Peças Total", "Peças e Serviços", "Auto Completo", "Peças Central"]
  }
  
  const nameOptions = companyNames[companyType] || companyNames["Oficina Mecânica"]
  const companyName = nameOptions[seed % nameOptions.length]
  
  return {
    cnpj: cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, "$1.$2.$3/$4-$5"),
    company_name: companyName,
    trade_name: companyName,
    company_type: companyType,
    status: "Ativa",
    opening_date: new Date(2010 + (seed % 14), seed % 12, (seed % 28) + 1).toISOString().split('T')[0],
    legal_nature: "Sociedade Empresária Limitada",
    main_activity: "4520-1/00 - Serviços de manutenção e reparação de veículos automotores",
    address: {
      street,
      number,
      complement: seed % 3 === 0 ? "Sala " + (seed % 10 + 1) : null,
      neighborhood: "Centro",
      city: city.name,
      state: city.state,
      zip_code: city.zip,
      country: "Brasil"
    },
    contact: {
      phone: `(${11 + (seed % 89)}) ${9000 + (seed % 9000)}-${1000 + (seed % 9000)}`,
      email: `contato@${companyName.toLowerCase().replace(/\s+/g, '')}.com.br`,
      website: `www.${companyName.toLowerCase().replace(/\s+/g, '')}.com.br`
    },
    financial: {
      capital: (10000 + (seed % 90000)).toFixed(2),
      revenue_range: seed % 3 === 0 ? "Até R$ 360.000" : seed % 3 === 1 ? "De R$ 360.000 a R$ 4.800.000" : "Acima de R$ 4.800.000",
      employees: (1 + (seed % 20)).toString()
    },
    registration: {
      state_registration: `${city.state}${seed.toString().padStart(8, '0')}`,
      municipal_registration: `${city.name.substr(0, 3).toUpperCase()}${seed.toString().padStart(6, '0')}`,
      suframa: null
    }
  }
}
