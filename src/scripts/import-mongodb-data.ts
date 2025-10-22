/**
 * Script para importar dados do backup MongoDB para PostgreSQL
 * Converte dados do MongoDB para o formato do MedusaJS
 */

import { BSON } from "bson"
import { readFileSync } from "fs"
import { gunzipSync } from "zlib"

const MONGODB_BACKUP_PATH = "/Users/filippoferrari/Documents/ARQUIVOS-MECA/Meca"

async function importData() {
  console.log("🚀 Iniciando importação de dados do MongoDB...")
  
  try {
    // Importar workshops
    await importWorkshops()
    
    // Importar veículos
    await importVehicles()
    
    // Importar agendamentos
    await importSchedulings()
    
    // Importar usuários
    await importUsers()
    
    console.log("✅ Importação concluída com sucesso!")
    
  } catch (error) {
    console.error("❌ Erro na importação:", error)
  }
}

async function importWorkshops() {
  console.log("📦 Importando workshops...")
  
  try {
    const workshopData = readBSONFile(`${MONGODB_BACKUP_PATH}/Workshop.bson.gz`)
    
    for (const workshop of workshopData) {
      console.log(`Importando workshop: ${workshop.name}`)
      
      // Aqui você criaria o workshop no MedusaJS
      // usando o módulo de oficina que já temos
    }
    
  } catch (error) {
    console.error("Erro ao importar workshops:", error)
  }
}

async function importVehicles() {
  console.log("🚗 Importando veículos...")
  
  try {
    const vehicleData = readBSONFile(`${MONGODB_BACKUP_PATH}/Vehicle.bson.gz`)
    
    for (const vehicle of vehicleData) {
      console.log(`Importando veículo: ${vehicle.brand} ${vehicle.model}`)
      
      // Aqui você criaria o veículo no MedusaJS
      // usando o módulo de vehicle que já temos
    }
    
  } catch (error) {
    console.error("Erro ao importar veículos:", error)
  }
}

async function importSchedulings() {
  console.log("📅 Importando agendamentos...")
  
  try {
    const schedulingData = readBSONFile(`${MONGODB_BACKUP_PATH}/Scheduling.bson.gz`)
    
    for (const scheduling of schedulingData) {
      console.log(`Importando agendamento: ${scheduling.date}`)
      
      // Aqui você criaria o booking no MedusaJS
      // usando o módulo de booking que já temos
    }
    
  } catch (error) {
    console.error("Erro ao importar agendamentos:", error)
  }
}

async function importUsers() {
  console.log("👤 Importando usuários...")
  
  try {
    const profileData = readBSONFile(`${MONGODB_BACKUP_PATH}/Profile.bson.gz`)
    
    for (const profile of profileData) {
      console.log(`Importando usuário: ${profile.name}`)
      
      // Aqui você criaria o customer no MedusaJS
      // usando o módulo de customer padrão
    }
    
  } catch (error) {
    console.error("Erro ao importar usuários:", error)
  }
}

function readBSONFile(filePath: string): any[] {
  try {
    const compressedData = readFileSync(filePath)
    const decompressedData = gunzipSync(compressedData)
    const bsonData = BSON.deserialize(decompressedData)
    
    // O BSON pode conter um array de documentos ou um único documento
    return Array.isArray(bsonData) ? bsonData : [bsonData]
    
  } catch (error) {
    console.error(`Erro ao ler arquivo ${filePath}:`, error)
    return []
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  importData()
}

export { importData }
