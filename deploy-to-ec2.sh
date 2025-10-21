#!/bin/bash
#
# Deploy Automatizado para EC2 AWS
# 
# Este script faz deploy completo da API MECA na EC2
# - Copia arquivos atualizados
# - Instala dependências
# - Reinicia PM2
# - Valida health check
#

set -e  # Parar em caso de erro

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Configurações
EC2_HOST="ec2-3-144-213-137.us-east-2.compute.amazonaws.com"
EC2_USER="ec2-user"
EC2_KEY="/Users/filippoferrari/Downloads/mecaServer EC2.pem"
REMOTE_DIR="~/meca"
API_URL="http://${EC2_HOST}:9000"

echo -e "${GREEN}🚀 MECA API - Deploy Automatizado${NC}"
echo "=================================================="
echo ""

# Verificar chave SSH
if [ ! -f "$EC2_KEY" ]; then
    echo -e "${RED}❌ Erro: Chave SSH não encontrada: $EC2_KEY${NC}"
    exit 1
fi

echo -e "${YELLOW}📦 Passo 1: Copiando arquivos...${NC}"
echo "=================================================="

# Criar backup remoto primeiro
ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "cd ${REMOTE_DIR} && tar czf ~/meca-backup-\$(date +%Y%m%d-%H%M%S).tar.gz ."
echo -e "${GREEN}✅ Backup criado${NC}"

# Copiar serviços
scp -i "$EC2_KEY" -r src/services/*.ts ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/services/
echo -e "${GREEN}✅ Serviços copiados${NC}"

# Copiar API routes
scp -i "$EC2_KEY" -r src/api/auth ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/api/
scp -i "$EC2_KEY" -r src/api/health ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/api/
scp -i "$EC2_KEY" -r src/api/middlewares ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/api/
echo -e "${GREEN}✅ API routes copiadas${NC}"

# Copiar subscribers
ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "mkdir -p ${REMOTE_DIR}/src/subscribers"
scp -i "$EC2_KEY" src/subscribers/*.ts ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/subscribers/
echo -e "${GREEN}✅ Subscribers copiados${NC}"

# Copiar jobs
ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "mkdir -p ${REMOTE_DIR}/src/jobs"
scp -i "$EC2_KEY" src/jobs/*.ts ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/jobs/
echo -e "${GREEN}✅ Jobs copiados${NC}"

# Copiar workflows
ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "mkdir -p ${REMOTE_DIR}/src/workflows"
scp -i "$EC2_KEY" src/workflows/*.ts ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/src/workflows/
echo -e "${GREEN}✅ Workflows copiados${NC}"

# Copiar package.json e medusa-config
scp -i "$EC2_KEY" package.json medusa-config.ts ${EC2_USER}@${EC2_HOST}:${REMOTE_DIR}/
echo -e "${GREEN}✅ Configurações copiadas${NC}"

echo ""
echo -e "${YELLOW}📥 Passo 2: Instalando dependências...${NC}"
echo "=================================================="

ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "cd ${REMOTE_DIR} && npm install 2>&1 | tail -10"
echo -e "${GREEN}✅ Dependências instaladas${NC}"

echo ""
echo -e "${YELLOW}🔄 Passo 3: Reiniciando API...${NC}"
echo "=================================================="

ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "cd ${REMOTE_DIR} && pm2 restart meca-api"
echo -e "${GREEN}✅ PM2 reiniciado${NC}"

echo ""
echo -e "${YELLOW}⏳ Aguardando API inicializar (15s)...${NC}"
sleep 15

echo ""
echo -e "${YELLOW}🏥 Passo 4: Verificando Health Check...${NC}"
echo "=================================================="

# Tentar health check 3 vezes
for i in {1..3}; do
    echo "Tentativa $i/3..."
    if curl -s -f "${API_URL}/health" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ API está saudável!${NC}"
        
        # Mostrar detalhes
        echo ""
        echo "Detalhes do Health Check:"
        curl -s "${API_URL}/health" | python3 -m json.tool 2>/dev/null || curl -s "${API_URL}/health"
        
        echo ""
        echo -e "${GREEN}🎉 DEPLOY CONCLUÍDO COM SUCESSO!${NC}"
        echo "=================================================="
        echo ""
        echo "📋 Informações:"
        echo "  - API URL: ${API_URL}"
        echo "  - Health: ${API_URL}/health"
        echo "  - Ready: ${API_URL}/health/ready"
        echo "  - Live: ${API_URL}/health/live"
        echo ""
        echo "📝 Próximos passos:"
        echo "  1. Testar login: ${API_URL}/auth/customer/token"
        echo "  2. Testar recuperação de senha: ${API_URL}/auth/customer/forgot-password"
        echo "  3. Testar agendamentos: ${API_URL}/store/bookings"
        echo ""
        
        exit 0
    fi
    
    echo "API não respondeu, aguardando..."
    sleep 5
done

echo -e "${RED}⚠️  API não respondeu ao health check${NC}"
echo "Verificando logs..."
ssh -i "$EC2_KEY" ${EC2_USER}@${EC2_HOST} "tail -50 ${REMOTE_DIR}/out.log"

exit 1

