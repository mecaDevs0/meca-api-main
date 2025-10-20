#!/bin/bash

# MECA v2.0 - Script de Deploy AWS EC2
# Este script automatiza o deploy do backend na sua instância EC2

set -e

echo "🚀 MECA v2.0 - Deploy AWS EC2"
echo "=============================="
echo ""

# Configurações
APP_DIR="/var/www/meca-backend"
PM2_APP_NAME="meca-api"

# 1. Atualizar sistema
echo "📦 Atualizando sistema..."
sudo apt-get update
sudo apt-get upgrade -y

# 2. Instalar dependências
echo "📦 Instalando dependências..."
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt-get install -y nodejs postgresql postgresql-contrib redis-server nginx

# 3. Instalar PM2 globalmente
echo "📦 Instalando PM2..."
sudo npm install -g pm2

# 4. Clonar ou atualizar repositório
echo "📥 Clonando repositório..."
if [ -d "$APP_DIR" ]; then
  cd $APP_DIR
  git pull
else
  sudo mkdir -p /var/www
  cd /var/www
  git clone <SEU_REPOSITORIO_GIT> meca-backend
  cd $APP_DIR
fi

# 5. Instalar dependências do projeto
echo "📦 Instalando dependências do projeto..."
npm install

# 6. Configurar variáveis de ambiente
echo "⚙️  Configurando ambiente..."
cat > .env << EOF
# Database
DATABASE_URL=postgresql://meca_user:meca_pass@localhost:5432/meca_db

# Redis
REDIS_URL=redis://localhost:6379

# JWT & Cookies
JWT_SECRET=$(openssl rand -base64 32)
COOKIE_SECRET=$(openssl rand -base64 32)

# CORS
STORE_CORS=http://localhost:3000,https://meca.com.br
ADMIN_CORS=http://localhost:3000,https://admin.meca.com.br

# AWS S3
AWS_ACCESS_KEY_ID=seu_access_key
AWS_SECRET_ACCESS_KEY=seu_secret_key
AWS_BUCKET=meca-images
AWS_REGION=us-east-1

# PagBank
PAGBANK_API_KEY=seu_api_key
PAGBANK_API_SECRET=seu_api_secret

# Firebase
FCM_SERVER_KEY=seu_server_key
FCM_PROJECT_ID=seu_project_id
EOF

# 7. Criar banco de dados
echo "🗄️  Configurando banco de dados..."
sudo -u postgres psql << EOSQL
CREATE DATABASE meca_db;
CREATE USER meca_user WITH ENCRYPTED PASSWORD 'meca_pass';
GRANT ALL PRIVILEGES ON DATABASE meca_db TO meca_user;
EOSQL

# 8. Executar migrações
echo "🔄 Executando migrações..."
npx medusa db:migrate

# 9. Build do projeto
echo "🔨 Building projeto..."
npm run build

# 10. Configurar PM2
echo "⚙️  Configurando PM2..."
pm2 delete $PM2_APP_NAME 2>/dev/null || true
pm2 start npm --name $PM2_APP_NAME -- start
pm2 save
pm2 startup

# 11. Configurar Nginx
echo "🌐 Configurando Nginx..."
sudo tee /etc/nginx/sites-available/meca-api > /dev/null << 'EOF'
server {
    listen 80;
    server_name api.meca.com.br;

    location / {
        proxy_pass http://localhost:9000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
EOF

sudo ln -sf /etc/nginx/sites-available/meca-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx

# 12. Configurar SSL com Let's Encrypt
echo "🔒 Configurando SSL..."
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api.meca.com.br --non-interactive --agree-tos -m admin@meca.com.br

echo ""
echo "✅ Deploy concluído com sucesso!"
echo ""
echo "📊 Serviços:"
echo "- Backend: http://api.meca.com.br"
echo "- Status PM2: pm2 status"
echo "- Logs: pm2 logs $PM2_APP_NAME"
echo ""
echo "🔧 Comandos úteis:"
echo "- Reiniciar: pm2 restart $PM2_APP_NAME"
echo "- Ver logs: pm2 logs $PM2_APP_NAME --lines 100"
echo "- Monitorar: pm2 monit"
echo ""

