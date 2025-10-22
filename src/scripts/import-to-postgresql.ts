/**
 * Script para importar dados do MongoDB diretamente para PostgreSQL
 */

import { BSON } from "bson"
import { readFileSync } from "fs"
import { Client } from "pg"
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

async function importToPostgreSQL() {
  console.log("🚀 Iniciando importação para PostgreSQL...")
  
  const client = new Client({
    host: 'localhost',
    port: 5432,
    database: 'medusa-learning-medusa',
    user: 'filippoferrari',
    password: ''
  })
  
  try {
    await client.connect()
    console.log("✅ Conectado ao PostgreSQL")
    
    // Importar customers
    console.log("\n👤 Importando customers...")
    const profiles = readBSONFile(`${MONGODB_BACKUP_PATH}/Profile.bson.gz`) as ProfileData[]
    
    for (const profile of profiles) {
      try {
        const result = await client.query(`
          INSERT INTO customer (id, email, first_name, last_name, phone, created_at, updated_at, metadata)
          VALUES ($1, $2, $3, $4, $5, NOW(), NOW(), $6)
          ON CONFLICT (id) DO NOTHING
        `, [
          profile._id,
          profile.Email,
          profile.FullName.split(' ')[0] || '',
          profile.FullName.split(' ').slice(1).join(' ') || '',
          profile.Phone,
          JSON.stringify({
            cpf: profile.Cpf,
            zipCode: profile.ZipCode,
            streetAddress: profile.StreetAddress,
            number: profile.Number,
            cityName: profile.CityName,
            stateName: profile.StateName,
            stateUf: profile.StateUf,
            neighborhood: profile.Neighborhood
          })
        ])
        
        console.log(`✅ Customer: ${profile.FullName} (${profile.Email})`)
        
      } catch (error) {
        console.error(`❌ Erro ao importar customer ${profile.FullName}:`, error.message)
      }
    }
    
    // Importar oficinas
    console.log("\n📦 Importando oficinas...")
    const workshops = readBSONFile(`${MONGODB_BACKUP_PATH}/Workshop.bson.gz`) as WorkshopData[]
    
    for (const workshop of workshops) {
      try {
        const result = await client.query(`
          INSERT INTO oficina (id, name, cnpj, email, phone, address, status, metadata)
          VALUES ($1, $2, $3, $4, $5, $6, $7, $8)
          ON CONFLICT (id) DO NOTHING
        `, [
          workshop._id,
          workshop.FullName,
          workshop.Cnpj,
          workshop.Email,
          workshop.Phone,
          JSON.stringify({
            zipCode: workshop.ZipCode,
            streetAddress: workshop.StreetAddress,
            number: workshop.Number,
            cityName: workshop.CityName,
            stateName: workshop.StateName,
            stateUf: workshop.StateUf,
            neighborhood: workshop.Neighborhood,
            latitude: workshop.Latitude,
            longitude: workshop.Longitude
          }),
          'aprovado', // Status padrão
          JSON.stringify({
            cpf: workshop.Cpf,
            login: workshop.Login,
            password: workshop.Password,
            rating: workshop.Rating,
            distance: workshop.Distance,
            companyName: workshop.CompanyName
          })
        ])
        
        console.log(`✅ Oficina: ${workshop.FullName} (${workshop.Email})`)
        
      } catch (error) {
        console.error(`❌ Erro ao importar oficina ${workshop.FullName}:`, error.message)
      }
    }
    
    // TODO: Importar veículos e agendamentos quando as tabelas estiverem disponíveis
    console.log("\n🚗 Veículos: Aguardando estrutura das tabelas")
    console.log("\n📅 Agendamentos: Aguardando estrutura das tabelas")
    
    console.log("\n✅ Importação concluída com sucesso!")
    
  } catch (error) {
    console.error("❌ Erro na importação:", error)
  } finally {
    await client.end()
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  importToPostgreSQL()
}

export { importToPostgreSQL }
