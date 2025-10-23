/**
 * Script simples para testar a leitura dos dados do MongoDB
 */

import { BSON } from "bson"
import { readFileSync } from "fs"
import { gunzipSync } from "zlib"

const MONGODB_BACKUP_PATH = "/Users/filippoferrari/Documents/ARQUIVOS-MECA/Meca"

function readBSONFile(filePath: string): any[] {
  try {
    console.log(`📖 Lendo arquivo: ${filePath}`)
    const compressedData = readFileSync(filePath)
    console.log(`📦 Tamanho comprimido: ${compressedData.length} bytes`)
    
    const decompressedData = gunzipSync(compressedData)
    console.log(`📦 Tamanho descomprimido: ${decompressedData.length} bytes`)
    
    // Ler múltiplos documentos BSON
    const documents: any[] = []
    let offset = 0
    
    while (offset < decompressedData.length) {
      try {
        // Tentar ler um documento BSON
        const documentSize = decompressedData.readInt32LE(offset)
        if (documentSize <= 0 || documentSize > decompressedData.length - offset) {
          break
        }
        
        const documentData = decompressedData.slice(offset, offset + documentSize)
        const document = BSON.deserialize(documentData)
        documents.push(document)
        
        offset += documentSize
        
      } catch (error) {
        console.log(`⚠️ Erro ao ler documento em offset ${offset}:`, error.message)
        break
      }
    }
    
    console.log(`📊 Total de documentos: ${documents.length}`)
    
    if (documents.length > 0) {
      console.log(`📋 Primeiro documento:`, Object.keys(documents[0]))
    }
    
    return documents
    
  } catch (error) {
    console.error(`❌ Erro ao ler arquivo ${filePath}:`, error)
    return []
  }
}

async function testReadData() {
  console.log("🚀 Testando leitura dos dados do MongoDB...")
  
  // Testar workshops
  console.log("\n📦 Testando workshops...")
  const workshops = readBSONFile(`${MONGODB_BACKUP_PATH}/Workshop.bson.gz`)
  console.log(`✅ Workshops encontrados: ${workshops.length}`)
  
  // Testar veículos
  console.log("\n🚗 Testando veículos...")
  const vehicles = readBSONFile(`${MONGODB_BACKUP_PATH}/Vehicle.bson.gz`)
  console.log(`✅ Veículos encontrados: ${vehicles.length}`)
  
  // Testar agendamentos
  console.log("\n📅 Testando agendamentos...")
  const schedulings = readBSONFile(`${MONGODB_BACKUP_PATH}/Scheduling.bson.gz`)
  console.log(`✅ Agendamentos encontrados: ${schedulings.length}`)
  
  // Testar perfis
  console.log("\n👤 Testando perfis...")
  const profiles = readBSONFile(`${MONGODB_BACKUP_PATH}/Profile.bson.gz`)
  console.log(`✅ Perfis encontrados: ${profiles.length}`)
  
  console.log("\n✅ Teste de leitura concluído!")
}

// Executar se chamado diretamente
if (require.main === module) {
  testReadData()
}

export { testReadData }

 */

import { BSON } from "bson"
import { readFileSync } from "fs"
import { gunzipSync } from "zlib"

const MONGODB_BACKUP_PATH = "/Users/filippoferrari/Documents/ARQUIVOS-MECA/Meca"

function readBSONFile(filePath: string): any[] {
  try {
    console.log(`📖 Lendo arquivo: ${filePath}`)
    const compressedData = readFileSync(filePath)
    console.log(`📦 Tamanho comprimido: ${compressedData.length} bytes`)
    
    const decompressedData = gunzipSync(compressedData)
    console.log(`📦 Tamanho descomprimido: ${decompressedData.length} bytes`)
    
    // Ler múltiplos documentos BSON
    const documents: any[] = []
    let offset = 0
    
    while (offset < decompressedData.length) {
      try {
        // Tentar ler um documento BSON
        const documentSize = decompressedData.readInt32LE(offset)
        if (documentSize <= 0 || documentSize > decompressedData.length - offset) {
          break
        }
        
        const documentData = decompressedData.slice(offset, offset + documentSize)
        const document = BSON.deserialize(documentData)
        documents.push(document)
        
        offset += documentSize
        
      } catch (error) {
        console.log(`⚠️ Erro ao ler documento em offset ${offset}:`, error.message)
        break
      }
    }
    
    console.log(`📊 Total de documentos: ${documents.length}`)
    
    if (documents.length > 0) {
      console.log(`📋 Primeiro documento:`, Object.keys(documents[0]))
    }
    
    return documents
    
  } catch (error) {
    console.error(`❌ Erro ao ler arquivo ${filePath}:`, error)
    return []
  }
}

async function testReadData() {
  console.log("🚀 Testando leitura dos dados do MongoDB...")
  
  // Testar workshops
  console.log("\n📦 Testando workshops...")
  const workshops = readBSONFile(`${MONGODB_BACKUP_PATH}/Workshop.bson.gz`)
  console.log(`✅ Workshops encontrados: ${workshops.length}`)
  
  // Testar veículos
  console.log("\n🚗 Testando veículos...")
  const vehicles = readBSONFile(`${MONGODB_BACKUP_PATH}/Vehicle.bson.gz`)
  console.log(`✅ Veículos encontrados: ${vehicles.length}`)
  
  // Testar agendamentos
  console.log("\n📅 Testando agendamentos...")
  const schedulings = readBSONFile(`${MONGODB_BACKUP_PATH}/Scheduling.bson.gz`)
  console.log(`✅ Agendamentos encontrados: ${schedulings.length}`)
  
  // Testar perfis
  console.log("\n👤 Testando perfis...")
  const profiles = readBSONFile(`${MONGODB_BACKUP_PATH}/Profile.bson.gz`)
  console.log(`✅ Perfis encontrados: ${profiles.length}`)
  
  console.log("\n✅ Teste de leitura concluído!")
}

// Executar se chamado diretamente
if (require.main === module) {
  testReadData()
}

export { testReadData }


