#!/bin/bash

# Script de deploy Docker para EC2
# Uso: ./deploy-docker.sh

set -e

echo "🚀 Iniciando deploy Docker MECA API para EC2..."

# Configurações
EC2_HOST="ec2-3-144-213-137.us-east-2.compute.amazonaws.com"
EC2_USER="ec2-user"
SSH_KEY="/Users/filippoferrari/Downloads/mecaServer EC2.pem"
REMOTE_DIR="/home/ec2-user/meca-docker"

# Cores
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}📦 Criando arquivo tar com código fonte...${NC}"
tar czf meca-api-docker.tar.gz \
    Dockerfile \
    .dockerignore \
    docker-compose.yml \
    nginx.conf \
    package.json \
    package-lock.json \
    tsconfig.json \
    medusa-config.ts \
    instrumentation.ts \
    ecosystem.config.js \
    src/ \
    .env

echo -e "${BLUE}✅ Tar criado com $(tar -tzf meca-api-docker.tar.gz | wc -l) arquivos${NC}"

echo -e "${BLUE}📤 Enviando para EC2...${NC}"
scp -i "$SSH_KEY" meca-api-docker.tar.gz "${EC2_USER}@${EC2_HOST}:~/"

echo -e "${BLUE}🔧 Configurando e iniciando no EC2...${NC}"
ssh -i "$SSH_KEY" "${EC2_USER}@${EC2_HOST}" << 'ENDSSH'
    set -e
    
    echo "🧹 Limpando instalação anterior..."
    rm -rf ~/meca-docker
    mkdir -p ~/meca-docker
    
    echo "📦 Extraindo arquivos..."
    tar xzf ~/meca-api-docker.tar.gz -C ~/meca-docker/
    cd ~/meca-docker
    
    echo "🐳 Instalando Docker (se necessário)..."
    if ! command -v docker &> /dev/null; then
        sudo yum update -y
        sudo yum install -y docker
        sudo systemctl start docker
        sudo systemctl enable docker
        sudo usermod -a -G docker $USER
    fi
    
    echo "🐳 Instalando Docker Compose (se necessário)..."
    if ! command -v docker-compose &> /dev/null; then
        sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        sudo chmod +x /usr/local/bin/docker-compose
    fi
    
    echo "🛑 Parando containers anteriores..."
    sudo docker-compose down 2>/dev/null || true
    
    echo "🔨 Construindo imagem Docker..."
    sudo docker-compose build --no-cache
    
    echo "🚀 Iniciando containers..."
    sudo docker-compose up -d
    
    echo "⏳ Aguardando API ficar pronta..."
    for i in {1..30}; do
        if sudo docker-compose exec -T meca-api curl -f http://localhost:9000/health/live 2>/dev/null; then
            echo "✅ API está funcionando!"
            break
        fi
        echo "Tentativa $i/30..."
        sleep 5
    done
    
    echo "📊 Status dos containers:"
    sudo docker-compose ps
    
    echo "📝 Logs recentes:"
    sudo docker-compose logs --tail=50 meca-api
    
    echo "🎉 Deploy concluído!"
ENDSSH

echo -e "${GREEN}✅ Deploy Docker completo!${NC}"
echo -e "${YELLOW}Para ver logs:${NC} ssh -i \"$SSH_KEY\" ${EC2_USER}@${EC2_HOST} 'cd ~/meca-docker && sudo docker-compose logs -f'"
echo -e "${YELLOW}Para parar:${NC} ssh -i \"$SSH_KEY\" ${EC2_USER}@${EC2_HOST} 'cd ~/meca-docker && sudo docker-compose down'"
echo -e "${YELLOW}Para restart:${NC} ssh -i \"$SSH_KEY\" ${EC2_USER}@${EC2_HOST} 'cd ~/meca-docker && sudo docker-compose restart'"

# Cleanup local
rm -f meca-api-docker.tar.gz

echo -e "${BLUE}🧪 Testando API...${NC}"
sleep 5
curl -s "http://${EC2_HOST}/health" | python3 -m json.tool || echo "API ainda não está respondendo, aguarde mais alguns segundos..."

echo -e "${GREEN}✨ Processo completo!${NC}"


# Script de deploy Docker para EC2
# Uso: ./deploy-docker.sh

set -e

echo "🚀 Iniciando deploy Docker MECA API para EC2..."

# Configurações
EC2_HOST="ec2-3-144-213-137.us-east-2.compute.amazonaws.com"
EC2_USER="ec2-user"
SSH_KEY="/Users/filippoferrari/Downloads/mecaServer EC2.pem"
REMOTE_DIR="/home/ec2-user/meca-docker"

# Cores
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}📦 Criando arquivo tar com código fonte...${NC}"
tar czf meca-api-docker.tar.gz \
    Dockerfile \
    .dockerignore \
    docker-compose.yml \
    nginx.conf \
    package.json \
    package-lock.json \
    tsconfig.json \
    medusa-config.ts \
    instrumentation.ts \
    ecosystem.config.js \
    src/ \
    .env

echo -e "${BLUE}✅ Tar criado com $(tar -tzf meca-api-docker.tar.gz | wc -l) arquivos${NC}"

echo -e "${BLUE}📤 Enviando para EC2...${NC}"
scp -i "$SSH_KEY" meca-api-docker.tar.gz "${EC2_USER}@${EC2_HOST}:~/"

echo -e "${BLUE}🔧 Configurando e iniciando no EC2...${NC}"
ssh -i "$SSH_KEY" "${EC2_USER}@${EC2_HOST}" << 'ENDSSH'
    set -e
    
    echo "🧹 Limpando instalação anterior..."
    rm -rf ~/meca-docker
    mkdir -p ~/meca-docker
    
    echo "📦 Extraindo arquivos..."
    tar xzf ~/meca-api-docker.tar.gz -C ~/meca-docker/
    cd ~/meca-docker
    
    echo "🐳 Instalando Docker (se necessário)..."
    if ! command -v docker &> /dev/null; then
        sudo yum update -y
        sudo yum install -y docker
        sudo systemctl start docker
        sudo systemctl enable docker
        sudo usermod -a -G docker $USER
    fi
    
    echo "🐳 Instalando Docker Compose (se necessário)..."
    if ! command -v docker-compose &> /dev/null; then
        sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        sudo chmod +x /usr/local/bin/docker-compose
    fi
    
    echo "🛑 Parando containers anteriores..."
    sudo docker-compose down 2>/dev/null || true
    
    echo "🔨 Construindo imagem Docker..."
    sudo docker-compose build --no-cache
    
    echo "🚀 Iniciando containers..."
    sudo docker-compose up -d
    
    echo "⏳ Aguardando API ficar pronta..."
    for i in {1..30}; do
        if sudo docker-compose exec -T meca-api curl -f http://localhost:9000/health/live 2>/dev/null; then
            echo "✅ API está funcionando!"
            break
        fi
        echo "Tentativa $i/30..."
        sleep 5
    done
    
    echo "📊 Status dos containers:"
    sudo docker-compose ps
    
    echo "📝 Logs recentes:"
    sudo docker-compose logs --tail=50 meca-api
    
    echo "🎉 Deploy concluído!"
ENDSSH

echo -e "${GREEN}✅ Deploy Docker completo!${NC}"
echo -e "${YELLOW}Para ver logs:${NC} ssh -i \"$SSH_KEY\" ${EC2_USER}@${EC2_HOST} 'cd ~/meca-docker && sudo docker-compose logs -f'"
echo -e "${YELLOW}Para parar:${NC} ssh -i \"$SSH_KEY\" ${EC2_USER}@${EC2_HOST} 'cd ~/meca-docker && sudo docker-compose down'"
echo -e "${YELLOW}Para restart:${NC} ssh -i \"$SSH_KEY\" ${EC2_USER}@${EC2_HOST} 'cd ~/meca-docker && sudo docker-compose restart'"

# Cleanup local
rm -f meca-api-docker.tar.gz

echo -e "${BLUE}🧪 Testando API...${NC}"
sleep 5
curl -s "http://${EC2_HOST}/health" | python3 -m json.tool || echo "API ainda não está respondendo, aguarde mais alguns segundos..."

echo -e "${GREEN}✨ Processo completo!${NC}"



