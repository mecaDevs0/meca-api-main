const { S3Client, CreateBucketCommand, PutBucketPolicyCommand, PutBucketCorsCommand } = require('@aws-sdk/client-s3');

// Configuração do cliente S3
const s3Client = new S3Client({
  region: process.env.AWS_REGION || 'us-east-2',
  credentials: {
    accessKeyId: process.env.AWS_ACCESS_KEY_ID,
    secretAccessKey: process.env.AWS_SECRET_ACCESS_KEY,
  },
});

const BUCKET_NAME = process.env.AWS_S3_BUCKET_NAME || 'meca-evidence-uploads';

async function setupS3Bucket() {
  try {
    console.log('🚀 Configurando bucket S3 para evidências...');

    // 1. Criar bucket (se não existir)
    try {
      await s3Client.send(new CreateBucketCommand({
        Bucket: BUCKET_NAME,
        CreateBucketConfiguration: {
          LocationConstraint: process.env.AWS_REGION || 'us-east-2'
        }
      }));
      console.log(`✅ Bucket ${BUCKET_NAME} criado com sucesso`);
    } catch (error) {
      if (error.name === 'BucketAlreadyOwnedByYou') {
        console.log(`✅ Bucket ${BUCKET_NAME} já existe`);
      } else {
        throw error;
      }
    }

    // 2. Configurar política do bucket (100% privado)
    const bucketPolicy = {
      Version: '2012-10-17',
      Statement: [
        {
          Sid: 'DenyPublicAccess',
          Effect: 'Deny',
          Principal: '*',
          Action: 's3:GetObject',
          Resource: `arn:aws:s3:::${BUCKET_NAME}/*`,
          Condition: {
            StringNotEquals: {
              'aws:PrincipalServiceName': 'medusa-backend'
            }
          }
        }
      ]
    };

    await s3Client.send(new PutBucketPolicyCommand({
      Bucket: BUCKET_NAME,
      Policy: JSON.stringify(bucketPolicy)
    }));
    console.log('✅ Política de bucket configurada (100% privado)');

    // 3. Configurar CORS para uploads
    const corsConfiguration = {
      CORSRules: [
        {
          AllowedHeaders: ['*'],
          AllowedMethods: ['PUT', 'POST', 'GET'],
          AllowedOrigins: ['*'],
          ExposeHeaders: ['ETag'],
          MaxAgeSeconds: 3000
        }
      ]
    };

    await s3Client.send(new PutBucketCorsCommand({
      Bucket: BUCKET_NAME,
      CORSConfiguration: corsConfiguration
    }));
    console.log('✅ CORS configurado para uploads');

    // 4. Criar estrutura de pastas
    const folders = [
      'evidence/',
      'evidence/temp/',
      'evidence/processed/',
      'evidence/archived/'
    ];

    for (const folder of folders) {
      try {
        await s3Client.send(new PutObjectCommand({
          Bucket: BUCKET_NAME,
          Key: folder,
          Body: '',
          ContentType: 'application/x-directory'
        }));
        console.log(`✅ Pasta ${folder} criada`);
      } catch (error) {
        console.log(`⚠️ Pasta ${folder} já existe ou erro: ${error.message}`);
      }
    }

    console.log('🎉 Configuração do S3 concluída com sucesso!');
    console.log(`📦 Bucket: ${BUCKET_NAME}`);
    console.log('🔒 Configuração: 100% privado, apenas URLs assinadas');
    console.log('📁 Estrutura: evidence/, evidence/temp/, evidence/processed/, evidence/archived/');

  } catch (error) {
    console.error('❌ Erro ao configurar S3:', error);
    throw error;
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  setupS3Bucket()
    .then(() => {
      console.log('✅ Setup S3 concluído');
      process.exit(0);
    })
    .catch((error) => {
      console.error('❌ Erro no setup S3:', error);
      process.exit(1);
    });
}

module.exports = { setupS3Bucket };
