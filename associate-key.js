/**
 * Script para associar publishable key com sales channel
 */

const { Client } = require('pg');

async function associateKey() {
  console.log('🚀 Associando publishable key com sales channel...');
  
  const client = new Client({
    host: 'meca-postgres-cluster.cpsysqk8qbd0.us-east-2.rds.amazonaws.com',
    port: 5432,
    database: 'meca-production',
    user: 'postgres',
    password: 'postgres',
    ssl: { rejectUnauthorized: false }
  });
  
  try {
    await client.connect();
    console.log('✅ Conectado ao PostgreSQL');
    
    // Buscar a publishable key existente
    const keyResult = await client.query(`
      SELECT id FROM api_key 
      WHERE type = 'publishable' 
      AND revoked_at IS NULL
      ORDER BY created_at DESC
      LIMIT 1
    `);
    
    if (keyResult.rows.length === 0) {
      console.log('❌ Nenhuma publishable key encontrada');
      return;
    }
    
    const keyId = keyResult.rows[0].id;
    console.log('✅ Publishable key encontrada:', keyId);
    
    // Buscar ou criar sales channel
    let channelId;
    const channelResult = await client.query(`
      SELECT id FROM sales_channel 
      WHERE name = 'MECA Store'
    `);
    
    if (channelResult.rows.length === 0) {
      channelId = 'sc_' + require('crypto').randomBytes(16).toString('hex');
      await client.query(`
        INSERT INTO sales_channel (id, name, description, is_disabled, created_at, updated_at)
        VALUES ($1, $2, $3, $4, NOW(), NOW())
      `, [channelId, 'MECA Store', 'Canal de vendas da MECA', false]);
      console.log('✅ Sales channel criado:', channelId);
    } else {
      channelId = channelResult.rows[0].id;
      console.log('✅ Sales channel encontrado:', channelId);
    }
    
    // Verificar se já existe associação
    const existingAssociation = await client.query(`
      SELECT * FROM publishable_api_key_sales_channel 
      WHERE publishable_key_id = $1 AND sales_channel_id = $2
    `, [keyId, channelId]);
    
    if (existingAssociation.rows.length > 0) {
      console.log('✅ Associação já existe');
      return;
    }
    
    // Criar associação
    const associationId = require('crypto').randomBytes(16).toString('hex');
    await client.query(`
      INSERT INTO publishable_api_key_sales_channel (id, publishable_key_id, sales_channel_id)
      VALUES ($1, $2, $3)
    `, [associationId, keyId, channelId]);
    
    console.log('✅ Associação criada com sucesso!');
    
  } catch (error) {
    console.error('❌ Erro:', error);
  } finally {
    await client.end();
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  associateKey();
}

module.exports = { associateKey };
