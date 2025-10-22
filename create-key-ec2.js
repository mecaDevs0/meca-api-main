/**
 * Script para criar publishable key na EC2
 */

const { Client } = require('pg');

async function createPublishableKey() {
  console.log('🚀 Criando publishable key na EC2...');
  
  // Parse DATABASE_URL
  const databaseUrl = process.env.DATABASE_URL || 'postgres://postgres:postgres@localhost:5432/medusa-learning-medusa';
  console.log('DATABASE_URL:', databaseUrl);
  const url = new URL(databaseUrl);
  console.log('Parsed URL:', {
    hostname: url.hostname,
    port: url.port,
    database: url.pathname.substring(1),
    user: url.username
  });
  
  const client = new Client({
    host: url.hostname,
    port: parseInt(url.port) || 5432,
    database: url.pathname.substring(1),
    user: url.username,
    password: url.password,
    ssl: url.searchParams.get('sslmode') === 'no-verify' ? { rejectUnauthorized: false } : false
  });
  
  try {
    await client.connect();
    console.log('✅ Conectado ao PostgreSQL');
    
    // Verificar se já existe uma publishable key
    const existingKey = await client.query(`
      SELECT * FROM api_key 
      WHERE type = 'publishable' 
      AND revoked_at IS NULL
    `);
    
    if (existingKey.rows.length > 0) {
      console.log('✅ Publishable key já existe:', existingKey.rows[0].token);
      return existingKey.rows[0].token;
    }
    
    // Criar nova publishable key
    const token = 'pk_' + require('crypto').randomBytes(32).toString('hex');
    const keyId = 'apk_' + require('crypto').randomBytes(16).toString('hex');
    const salt = require('crypto').randomBytes(16).toString('hex');
    
    const redacted = token.substring(0, 8) + '***' + token.substring(token.length - 3);
    
    // Buscar um usuário existente para created_by
    const userResult = await client.query('SELECT id FROM "user" LIMIT 1');
    const createdBy = userResult.rows.length > 0 ? userResult.rows[0].id : null;
    
    const result = await client.query(`
      INSERT INTO api_key (id, title, token, type, salt, redacted, created_by, created_at, updated_at)
      VALUES ($1, $2, $3, $4, $5, $6, $7, NOW(), NOW())
    `, [keyId, 'MECA API Key', token, 'publishable', salt, redacted, createdBy]);
    
    console.log('✅ Publishable key criada:', token);
    
    // Criar sales channel se não existir
    const existingChannel = await client.query(`
      SELECT * FROM sales_channel 
      WHERE name = 'MECA Store'
    `);
    
    if (existingChannel.rows.length === 0) {
      const channelId = 'sc_' + require('crypto').randomBytes(16).toString('hex');
      
      await client.query(`
        INSERT INTO sales_channel (id, name, description, is_disabled, created_at, updated_at)
        VALUES ($1, $2, $3, $4, NOW(), NOW())
      `, [channelId, 'MECA Store', 'Canal de vendas da MECA', false]);
      
      console.log('✅ Sales channel criado: MECA Store');
      
      // Associar key com sales channel
      await client.query(`
        INSERT INTO publishable_api_key_sales_channel (id, publishable_key_id, sales_channel_id)
        VALUES ($1, $2, $3)
      `, [require('crypto').randomBytes(16).toString('hex'), keyId, channelId]);
      
      console.log('✅ Publishable key associada ao sales channel');
    }
    
    return token;
    
  } catch (error) {
    console.error('❌ Erro ao criar publishable key:', error);
    throw error;
  } finally {
    await client.end();
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  createPublishableKey()
    .then(token => {
      console.log('\n🎉 Publishable key criada com sucesso!');
      console.log('Token:', token);
      process.exit(0);
    })
    .catch(error => {
      console.error('❌ Erro:', error);
      process.exit(1);
    });
}

module.exports = { createPublishableKey };
