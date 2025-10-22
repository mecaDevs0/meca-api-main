import { MedusaContainer } from "@medusajs/framework"
import { exec } from "child_process"
import * as fs from "fs"
import * as path from "path"
import { promisify } from "util"

const execAsync = promisify(exec)

/**
 * Scheduled Job: Database Backup
 * 
 * Faz backup automático do banco PostgreSQL
 * Executa diariamente às 2:00 AM
 */

export default async function backupDatabaseJob(container: MedusaContainer) {
  const logger = container.resolve("logger")
  
  try {
    logger.info("Starting database backup")
    
    const databaseUrl = process.env.DATABASE_URL
    if (!databaseUrl) {
      throw new Error('DATABASE_URL not configured')
    }

    // Parse database URL
    const dbMatch = databaseUrl.match(/postgres:\/\/([^:]+):([^@]+)@([^:]+):(\d+)\/([^?]+)/)
    if (!dbMatch) {
      throw new Error('Invalid DATABASE_URL format')
    }

    const [, user, password, host, port, database] = dbMatch

    // Criar diretório de backup se não existir
    const backupDir = path.join(process.cwd(), 'backups')
    if (!fs.existsSync(backupDir)) {
      fs.mkdirSync(backupDir, { recursive: true })
    }

    // Nome do arquivo de backup
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').split('T')[0]
    const backupFile = path.join(backupDir, `meca-backup-${timestamp}.sql`)

    // Executar pg_dump
    const command = `PGPASSWORD="${password}" pg_dump -h ${host} -p ${port} -U ${user} -d ${database} -F p -f ${backupFile}`

    await execAsync(command)

    // Verificar se backup foi criado
    const stats = fs.statSync(backupFile)
    const sizeInMB = (stats.size / (1024 * 1024)).toFixed(2)

    logger.info(`Database backup created: ${backupFile} (${sizeInMB} MB)`)

    // Limpar backups antigos (manter últimos 7 dias)
    await this.cleanOldBackups(backupDir, 7)

    return {
      success: true,
      file: backupFile,
      size: `${sizeInMB} MB`,
      timestamp: new Date().toISOString()
    }
  } catch (error) {
    logger.error("Error in database backup job:", error)
    throw error
  }
}

/**
 * Remove backups mais antigos que X dias
 */
async function cleanOldBackups(backupDir: string, daysToKeep: number): Promise<void> {
  try {
    const files = fs.readdirSync(backupDir)
    const now = Date.now()
    const maxAge = daysToKeep * 24 * 60 * 60 * 1000
    let deleted = 0

    for (const file of files) {
      if (!file.startsWith('meca-backup-')) continue

      const filePath = path.join(backupDir, file)
      const stats = fs.statSync(filePath)
      const age = now - stats.mtimeMs

      if (age > maxAge) {
        fs.unlinkSync(filePath)
        deleted++
      }
    }

    if (deleted > 0) {
      console.log(`Deleted ${deleted} old backup files`)
    }
  } catch (error) {
    console.error('Error cleaning old backups:', error)
  }
}

export const config = {
  name: "backup-database",
  schedule: "0 2 * * *", // Diariamente às 2:00 AM
  data: {},
}

