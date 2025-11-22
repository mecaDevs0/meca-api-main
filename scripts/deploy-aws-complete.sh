#!/bin/bash

# Script completo de deploy para AWS
# Configura EC2, RDS, S3, PagBank e todas as integrações

set -e

echo "🚀 Iniciando deploy completo do MECA na AWS..."

# 1. Configurar variáveis de ambiente
echo "📝 Configurando variáveis de ambiente..."

# AWS Configuration
export AWS_REGION="us-east-2"
export AWS_S3_BUCKET_NAME="meca-evidence-uploads"
export AWS_ACCESS_KEY_ID="YOUR_ACCESS_KEY"
export AWS_SECRET_ACCESS_KEY="YOUR_SECRET_KEY"

# Database Configuration
export DB_HOST="your-rds-endpoint.amazonaws.com"
export DB_PORT="5432"
export DB_NAME="meca_production"
export DB_USER="meca_user"
export DB_PASSWORD="your_secure_password"

# PagBank Configuration
export PAGBANK_API_URL="https://api.pagseguro.com"
export PAGBANK_TOKEN="your_pagbank_token"
export PAGBANK_WEBHOOK_SECRET="your_webhook_secret"
export WEBHOOK_URL="https://api.meca.com/webhook/pagbank"

# Application Configuration
export NODE_ENV="production"
export PORT="9000"
export JWT_SECRET="your_jwt_secret"
export ENCRYPTION_KEY="your_encryption_key"

echo "✅ Variáveis de ambiente configuradas"

# 2. Instalar dependências
echo "📦 Instalando dependências..."
npm install
npm install -g pm2

# 3. Configurar banco de dados RDS
echo "🗄️ Configurando banco de dados RDS..."
psql -h $DB_HOST -U $DB_USER -d $DB_NAME -f scripts/setup-database.sql

# 4. Configurar S3
echo "☁️ Configurando S3..."
node scripts/setup-aws-s3.js

# 5. Configurar webhook PagBank
echo "💳 Configurando webhook PagBank..."
node scripts/setup-pagbank-webhook.js

# 6. Compilar aplicação
echo "🔨 Compilando aplicação..."
npm run build

# 7. Configurar PM2
echo "⚙️ Configurando PM2..."
cat > ecosystem.config.js << EOF
module.exports = {
  apps: [{
    name: 'meca-api',
    script: 'dist/index.js',
    instances: 'max',
    exec_mode: 'cluster',
    env: {
      NODE_ENV: 'production',
      PORT: 9000
    },
    error_file: './logs/err.log',
    out_file: './logs/out.log',
    log_file: './logs/combined.log',
    time: true
  }]
}
EOF

# 8. Criar diretórios de logs
mkdir -p logs
mkdir -p uploads/temp

# 9. Configurar nginx (se necessário)
echo "🌐 Configurando nginx..."
cat > /etc/nginx/sites-available/meca-api << EOF
server {
    listen 80;
    server_name api.meca.com;

    location / {
        proxy_pass http://localhost:9000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# 10. Iniciar aplicação
echo "🚀 Iniciando aplicação..."
pm2 start ecosystem.config.js
pm2 save
pm2 startup

# 11. Configurar backup automático
echo "💾 Configurando backup automático..."
cat > /etc/cron.d/meca-backup << EOF
# Backup diário do banco de dados
0 2 * * * pg_dump -h $DB_HOST -U $DB_USER -d $DB_NAME > /backups/meca_\$(date +\%Y\%m\%d).sql

# Limpeza de logs antigos
0 3 * * * find /var/log/meca -name "*.log" -mtime +30 -delete

# Limpeza de arquivos temporários
0 4 * * * find /app/uploads/temp -name "*" -mtime +1 -delete
EOF

# 12. Configurar monitoramento
echo "📊 Configurando monitoramento..."
pm2 install pm2-logrotate
pm2 set pm2-logrotate:max_size 10M
pm2 set pm2-logrotate:retain 7

# 13. Testar aplicação
echo "🧪 Testando aplicação..."
sleep 10
curl -f http://localhost:9000/health || {
    echo "❌ Aplicação não está respondendo"
    exit 1
}

# 14. Configurar SSL (se necessário)
echo "🔒 Configurando SSL..."
# Certbot para Let's Encrypt (se necessário)
# certbot --nginx -d api.meca.com

# 15. Configurar firewall
echo "🔥 Configurando firewall..."
ufw allow 22
ufw allow 80
ufw allow 443
ufw allow 9000
ufw --force enable

# 16. Verificar status final
echo "✅ Verificando status final..."
pm2 status
pm2 logs --lines 10

echo "🎉 Deploy completo realizado com sucesso!"
echo ""
echo "📋 Resumo da configuração:"
echo "   • API: http://api.meca.com:9000"
echo "   • Database: $DB_HOST:$DB_PORT/$DB_NAME"
echo "   • S3 Bucket: $AWS_S3_BUCKET_NAME"
echo "   • PagBank Webhook: $WEBHOOK_URL"
echo "   • PM2 Status: $(pm2 jlist | jq '.[0].pm2_env.status')"
echo ""
echo "🔧 Comandos úteis:"
echo "   • Ver logs: pm2 logs meca-api"
echo "   • Reiniciar: pm2 restart meca-api"
echo "   • Status: pm2 status"
echo "   • Monitor: pm2 monit"
echo ""
echo "🚀 MECA está rodando em produção!"



















