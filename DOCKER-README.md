# 🐳 MECA API - Deploy Docker

Solução completa containerizada para a API MECA com Docker.

## 📋 Pré-requisitos

- Docker 20.10+
- Docker Compose 2.0+
- Make (opcional, facilita comandos)

## 🚀 Início Rápido

### Local (Desenvolvimento)

```bash
# 1. Configurar variáveis de ambiente
cp .env.example .env
# Edite o .env com suas configurações

# 2. Iniciar ambiente de desenvolvimento
make dev
# OU
docker-compose -f docker-compose.dev.yml up

# 3. Testar
make test
# OU
curl http://localhost:9000/health
```

### Produção (EC2)

```bash
# Deploy automático para EC2
make deploy
# OU
./deploy-docker.sh
```

## 🛠️ Comandos Disponíveis

```bash
make help          # Lista todos os comandos
make build         # Build da imagem
make up            # Inicia containers
make down          # Para containers
make logs          # Ver logs em tempo real
make restart       # Restart containers
make clean         # Remove tudo
make deploy        # Deploy para EC2
make dev           # Ambiente dev com hot reload
make test          # Testa API
make shell         # Shell no container
make rebuild       # Rebuild completo
make status        # Status e health check
```

## 📁 Estrutura Docker

```
learning-medusa/
├── Dockerfile              # Imagem multi-stage otimizada
├── .dockerignore          # Arquivos ignorados
├── docker-compose.yml     # Produção
├── docker-compose.dev.yml # Desenvolvimento
├── nginx.conf             # Reverse proxy + rate limiting
├── deploy-docker.sh       # Script de deploy EC2
└── Makefile              # Comandos facilitados
```

## 🏗️ Arquitetura Docker

```
┌─────────────────┐
│  Nginx (80/443) │  ← Reverse Proxy + Rate Limit
└────────┬────────┘
         │
┌────────▼────────┐
│  MECA API       │  ← MedusaJS (9000)
│  (Node 20)      │
└────────┬────────┘
         │
┌────────▼────────┐
│  Redis Cache    │  ← Cache (6379)
└─────────────────┘
         │
┌────────▼────────┐
│  AWS RDS        │  ← PostgreSQL (externo)
└─────────────────┘
```

## ⚙️ Variáveis de Ambiente

Criar arquivo `.env`:

```env
# Database
DATABASE_URL=postgresql://user:pass@host:5432/meca

# Redis
REDIS_URL=redis://redis:6379

# Security
JWT_SECRET=your-super-secret-jwt-key-here
COOKIE_SECRET=your-super-secret-cookie-key-here

# CORS
STORE_CORS=https://app.mecabr.com
ADMIN_CORS=https://admin.mecabr.com

# Email
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_EMAIL=suporte@mecabr.com
SMTP_PASSWORD=your-app-password
```

## 🔍 Monitoramento

### Ver logs
```bash
# Todos os serviços
docker-compose logs -f

# Apenas API
docker-compose logs -f meca-api

# Últimas 100 linhas
docker-compose logs --tail=100 meca-api
```

### Health checks
```bash
# Local
curl http://localhost:9000/health
curl http://localhost:9000/health/live
curl http://localhost:9000/health/ready

# Produção
curl http://ec2-3-144-213-137.us-east-2.compute.amazonaws.com/health
```

### Status containers
```bash
docker-compose ps
docker stats
```

## 🔧 Troubleshooting

### Container não inicia
```bash
# Ver logs detalhados
docker-compose logs meca-api

# Rebuild sem cache
docker-compose build --no-cache
docker-compose up
```

### Erro de permissão
```bash
# Adicionar usuário ao grupo docker
sudo usermod -aG docker $USER
# Logout e login novamente
```

### Limpar tudo
```bash
make clean
# OU
docker-compose down -v
docker system prune -af
```

## 📊 Performance

### Otimizações incluídas:
- ✅ Multi-stage build (imagem 60% menor)
- ✅ Cache de dependências otimizado
- ✅ Health checks automáticos
- ✅ Rate limiting no Nginx
- ✅ Redis cache configurado
- ✅ Logs rotativos (max 3 x 10MB)
- ✅ Restart automático
- ✅ User não-root (segurança)

## 🔐 Segurança

### Headers de segurança (Nginx):
- X-Frame-Options
- X-Content-Type-Options
- X-XSS-Protection
- Referrer-Policy

### Rate Limiting:
- API geral: 10 req/s
- Endpoints auth: 5 req/min
- Burst configurável

## 🚀 Deploy na EC2

O script `deploy-docker.sh` faz:

1. ✅ Cria arquivo tar com código
2. ✅ Envia para EC2 via SCP
3. ✅ Instala Docker/Docker Compose (se necessário)
4. ✅ Para containers anteriores
5. ✅ Build da nova imagem
6. ✅ Inicia containers
7. ✅ Aguarda health check
8. ✅ Mostra status e logs

### Primeiro deploy:
```bash
./deploy-docker.sh
```

### Deploys subsequentes:
```bash
# Mesma coisa - script é idempotente
./deploy-docker.sh
```

## 📝 Notas

- **Sem PM2**: Docker gerencia processos
- **Sem ts-node**: Build acontece na imagem
- **Sem problemas de módulos**: Tudo isolado
- **100% reproducível**: Mesma imagem local e produção

## 🎯 Próximos Passos

1. Deploy inicial: `./deploy-docker.sh`
2. Configurar DNS apontando para EC2
3. Adicionar SSL/HTTPS no Nginx
4. Configurar backups automatizados
5. Implementar CI/CD

---

**Criado por:** MECA Team  
**Última atualização:** 2025-10-21


