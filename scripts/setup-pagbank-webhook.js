const axios = require('axios');

// Configurações do PagBank
const PAGBANK_API_URL = process.env.PAGBANK_API_URL || 'https://api.pagseguro.com';
const PAGBANK_TOKEN = process.env.PAGBANK_TOKEN;
const WEBHOOK_URL = process.env.WEBHOOK_URL || 'https://api.meca.com/webhook/pagbank';
const WEBHOOK_SECRET = process.env.PAGBANK_WEBHOOK_SECRET;

async function setupPagBankWebhook() {
  try {
    console.log('🚀 Configurando webhook PagBank...');

    if (!PAGBANK_TOKEN) {
      throw new Error('PAGBANK_TOKEN não configurado');
    }

    if (!WEBHOOK_URL) {
      throw new Error('WEBHOOK_URL não configurado');
    }

    // 1. Verificar se webhook já existe
    try {
      const existingWebhooks = await axios.get(`${PAGBANK_API_URL}/webhooks`, {
        headers: {
          'Authorization': `Bearer ${PAGBANK_TOKEN}`,
          'Content-Type': 'application/json'
        }
      });

      const existingWebhook = existingWebhooks.data.find(
        webhook => webhook.url === WEBHOOK_URL
      );

      if (existingWebhook) {
        console.log(`✅ Webhook já existe: ${existingWebhook.id}`);
        return existingWebhook;
      }
    } catch (error) {
      console.log('⚠️ Erro ao verificar webhooks existentes:', error.message);
    }

    // 2. Criar novo webhook
    const webhookConfig = {
      url: WEBHOOK_URL,
      events: [
        'payment.approved',
        'payment.denied',
        'payment.pending',
        'payment.cancelled'
      ],
      description: 'MECA - Webhook para confirmação de pagamentos',
      secret: WEBHOOK_SECRET
    };

    const response = await axios.post(`${PAGBANK_API_URL}/webhooks`, webhookConfig, {
      headers: {
        'Authorization': `Bearer ${PAGBANK_TOKEN}`,
        'Content-Type': 'application/json'
      }
    });

    console.log('✅ Webhook PagBank criado com sucesso!');
    console.log(`🔗 URL: ${WEBHOOK_URL}`);
    console.log(`📡 Eventos: ${webhookConfig.events.join(', ')}`);
    console.log(`🔑 Secret: ${WEBHOOK_SECRET ? 'Configurado' : 'Não configurado'}`);

    return response.data;

  } catch (error) {
    console.error('❌ Erro ao configurar webhook PagBank:', error);
    throw error;
  }
}

async function testWebhook() {
  try {
    console.log('🧪 Testando webhook...');

    const testPayload = {
      event: 'payment.approved',
      data: {
        id: 'test_payment_123',
        status: 'approved',
        amount: 100.00,
        metadata: {
          booking_id: 'test_booking_123'
        }
      }
    };

    const response = await axios.post(WEBHOOK_URL, testPayload, {
      headers: {
        'Content-Type': 'application/json',
        'x-pagbank-signature': 'test_signature'
      }
    });

    console.log('✅ Webhook testado com sucesso!');
    console.log(`📊 Status: ${response.status}`);
    console.log(`📝 Response: ${JSON.stringify(response.data)}`);

  } catch (error) {
    console.error('❌ Erro ao testar webhook:', error);
    throw error;
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  const command = process.argv[2];

  if (command === 'test') {
    testWebhook()
      .then(() => {
        console.log('✅ Teste do webhook concluído');
        process.exit(0);
      })
      .catch((error) => {
        console.error('❌ Erro no teste do webhook:', error);
        process.exit(1);
      });
  } else {
    setupPagBankWebhook()
      .then(() => {
        console.log('✅ Setup PagBank concluído');
        process.exit(0);
      })
      .catch((error) => {
        console.error('❌ Erro no setup PagBank:', error);
        process.exit(1);
      });
  }
}

module.exports = { setupPagBankWebhook, testWebhook };













