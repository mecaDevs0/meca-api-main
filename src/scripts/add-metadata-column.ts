import { Pool } from 'pg'

export default async function addMetadataColumn() {
  const pool = new Pool({
    connectionString: process.env.DATABASE_URL
  })

  try {
    await pool.query('ALTER TABLE oficina ADD COLUMN IF NOT EXISTS metadata jsonb')
    console.log('✅ Coluna metadata adicionada à tabela oficina!')
  } catch (error) {
    console.error('❌ Erro ao adicionar coluna:', error)
  } finally {
    await pool.end()
  }
}

