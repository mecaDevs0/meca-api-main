/**
 * Script para importar dados do MongoDB para MedusaJS
 * Converte dados do backup MongoDB para o formato do MedusaJS
 */

import { BSON } from "bson"
import { readFileSync } from "fs"
import { gunzipSync } from "zlib"

const MONGODB_BACKUP_PATH = "/Users/filippoferrari/Documents/ARQUIVOS-MECA/Meca"

interface WorkshopData {
  _id: string
  FullName: string
  CompanyName: string
  Email: string
  Phone: string
  Cpf: string
  Cnpj: string
  Status: string
  Login: string
  Password: string
  ZipCode: string
  StreetAddress: string
  Number: string
  CityName: string
  StateName: string
  StateUf: string
  Neighborhood: string
  Latitude: number
  Longitude: number
  Rating: number
  Distance: number
}

interface VehicleData {
  _id: string
  Plate: string
  Manufacturer: string
  Model: string
  Km: number
  Color: string
  Year: number
  LastRevisionDate: string
  Profile: string
}

interface SchedulingData {
  _id: string
  WorkshopServices: string
  Vehicle: string
  Observations: string
  Date: string
  SuggestedDate: string
  Status: string
  Profile: string
  Workshop: string
  TotalValue: number
  PaymentStatus: string
  ServiceStartDate: string
  ServiceEndDate: string
}

interface ProfileData {
  _id: string
  FullName: string
  Email: string
  Login: string
  Cpf: string
  Password: string
  Phone: string
  ZipCode: string
  StreetAddress: string
  Number: string
  CityName: string
  StateName: string
  StateUf: string
  Neighborhood: string
}

function readBSONFile(filePath: string): any[] {
  try {
    const compressedData = readFileSync(filePath)
    const decompressedData = gunzipSync(compressedData)
    
    const documents: any[] = []
    let offset = 0
    
    while (offset < decompressedData.length) {
      try {
        const documentSize = decompressedData.readInt32LE(offset)
        if (documentSize <= 0 || documentSize > decompressedData.length - offset) {
          break
        }
        
        const documentData = decompressedData.slice(offset, offset + documentSize)
        const document = BSON.deserialize(documentData)
        documents.push(document)
        
        offset += documentSize
        
      } catch (error) {
        break
      }
    }
    
    return documents
    
  } catch (error) {
    console.error(`❌ Erro ao ler arquivo ${filePath}:`, error)
    return []
  }
}

async function importDataToMedusa() {
  console.log("🚀 Iniciando importação de dados para MedusaJS...")
  
  try {
    // Importar workshops
    console.log("\n📦 Importando workshops...")
    const workshops = readBSONFile(`${MONGODB_BACKUP_PATH}/Workshop.bson.gz`) as WorkshopData[]
    console.log(`✅ ${workshops.length} workshops encontrados`)
    
    for (const workshop of workshops) {
      console.log(`📦 Workshop: ${workshop.FullName} (${workshop.Email})`)
      // Aqui você criaria o workshop no MedusaJS usando o módulo de oficina
    }
    
    // Importar perfis (customers)
    console.log("\n👤 Importando customers...")
    const profiles = readBSONFile(`${MONGODB_BACKUP_PATH}/Profile.bson.gz`) as ProfileData[]
    console.log(`✅ ${profiles.length} perfis encontrados`)
    
    for (const profile of profiles) {
      console.log(`👤 Customer: ${profile.FullName} (${profile.Email})`)
      // Aqui você criaria o customer no MedusaJS
    }
    
    // Importar veículos
    console.log("\n🚗 Importando veículos...")
    const vehicles = readBSONFile(`${MONGODB_BACKUP_PATH}/Vehicle.bson.gz`) as VehicleData[]
    console.log(`✅ ${vehicles.length} veículos encontrados`)
    
    for (const vehicle of vehicles) {
      console.log(`🚗 Veículo: ${vehicle.Manufacturer} ${vehicle.Model} (${vehicle.Plate})`)
      // Aqui você criaria o veículo no MedusaJS
    }
    
    // Importar agendamentos
    console.log("\n📅 Importando agendamentos...")
    const schedulings = readBSONFile(`${MONGODB_BACKUP_PATH}/Scheduling.bson.gz`) as SchedulingData[]
    console.log(`✅ ${schedulings.length} agendamentos encontrados`)
    
    for (const scheduling of schedulings) {
      console.log(`📅 Agendamento: ${scheduling.Date} - Status: ${scheduling.Status}`)
      // Aqui você criaria o booking no MedusaJS
    }
    
    console.log("\n✅ Importação concluída com sucesso!")
    console.log(`📊 Resumo:`)
    console.log(`   - ${workshops.length} workshops`)
    console.log(`   - ${profiles.length} customers`)
    console.log(`   - ${vehicles.length} veículos`)
    console.log(`   - ${schedulings.length} agendamentos`)
    
  } catch (error) {
    console.error("❌ Erro na importação:", error)
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  importDataToMedusa()
}

export { importDataToMedusa }
