const { Pool } = require('pg');

// Configuração do banco RDS
const pool = new Pool({
  host: 'meca-database.cx8x8x8x8x8x8.us-east-2.rds.amazonaws.com', // Substitua pela URL real do RDS
  port: 5432,
  database: 'meca_production',
  user: 'meca_user',
  password: 'meca_password', // Substitua pela senha real
  ssl: {
    rejectUnauthorized: false
  }
});

async function updateWorkshopsInstallment() {
  const client = await pool.connect();
  
  try {
    console.log('🔄 Iniciando atualização das oficinas para aceitar parcelamento...');
    
    // Primeiro, verificar se a coluna existe
    const checkColumnQuery = `
      SELECT column_name 
      FROM information_schema.columns 
      WHERE table_name = 'workshops' 
      AND column_name = 'accepts_installment'
    `;
    
    const columnExists = await client.query(checkColumnQuery);
    
    if (columnExists.rows.length === 0) {
      console.log('📝 Criando coluna accepts_installment...');
      
      // Criar a coluna se não existir
      await client.query(`
        ALTER TABLE workshops 
        ADD COLUMN accepts_installment BOOLEAN DEFAULT false
      `);
      
      console.log('✅ Coluna accepts_installment criada com sucesso!');
    } else {
      console.log('✅ Coluna accepts_installment já existe!');
    }
    
    // Atualizar todas as oficinas para aceitar parcelamento
    const updateQuery = `
      UPDATE workshops 
      SET accepts_installment = true 
      WHERE accepts_installment IS NULL OR accepts_installment = false
    `;
    
    const result = await client.query(updateQuery);
    
    console.log(`✅ ${result.rowCount} oficinas atualizadas para aceitar parcelamento!`);
    
    // Verificar o resultado
    const checkQuery = `
      SELECT 
        COUNT(*) as total_workshops,
        COUNT(CASE WHEN accepts_installment = true THEN 1 END) as workshops_with_installment
      FROM workshops
    `;
    
    const stats = await client.query(checkQuery);
    const { total_workshops, workshops_with_installment } = stats.rows[0];
    
    console.log(`📊 Estatísticas:`);
    console.log(`   - Total de oficinas: ${total_workshops}`);
    console.log(`   - Oficinas que aceitam parcelamento: ${workshops_with_installment}`);
    console.log(`   - Percentual: ${((workshops_with_installment / total_workshops) * 100).toFixed(1)}%`);
    
  } catch (error) {
    console.error('❌ Erro ao atualizar oficinas:', error);
    throw error;
  } finally {
    client.release();
  }
}

async function main() {
  try {
    await updateWorkshopsInstallment();
    console.log('🎉 Atualização concluída com sucesso!');
    process.exit(0);
  } catch (error) {
    console.error('💥 Falha na atualização:', error);
    process.exit(1);
  }
}

// Executar apenas se chamado diretamente
if (require.main === module) {
  main();
}

module.exports = { updateWorkshopsInstallment };



















