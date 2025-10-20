# 🚗 MECA API Backend

API backend para a plataforma MECA - marketplace de serviços automotivos, construída com MedusaJS v2.

## 🎯 Funcionalidades

- **Autenticação:** JWT, Google OAuth, Apple OAuth
- **Gestão de Oficinas:** CRUD, aprovação, status
- **Gestão de Serviços:** Master services, produtos
- **Gestão de Veículos:** CRUD de veículos por cliente
- **Gestão de Agendamentos:** Booking system completo
- **Pagamentos:** Integração PagBank
- **Notificações:** FCM push notifications
- **Comissões:** Sistema de comissão MECA (10%)
- **Relatórios:** Analytics e métricas

## 🛠️ Tecnologias

- **Backend:** Node.js + TypeScript
- **Framework:** MedusaJS v2
- **Banco de Dados:** PostgreSQL
- **Cache:** Redis
- **Storage:** AWS S3
- **ORM:** MikroORM
- **Autenticação:** JWT, OAuth
- **Pagamentos:** PagBank API
- **Notificações:** Firebase Cloud Messaging

## 🚀 Como Executar

### Pré-requisitos
- Node.js 18+
- PostgreSQL 14+
- Redis 6+
- Yarn ou NPM

### Instalação
```bash
# Clone o repositório
git clone https://github.com/mecaDevs0/meca-api.git
cd meca-api

# Instale as dependências
yarn install

# Configure as variáveis de ambiente
cp .env.example .env
# Edite o .env com suas configurações

# Execute as migrações
yarn db:migrate

# Popule o banco com dados iniciais
yarn seed

# Inicie o servidor
yarn dev
```

### Variáveis de Ambiente
```env
# Database
DATABASE_URL=postgres://postgres@localhost/medusa-learning-medusa

# Redis
REDIS_URL=redis://localhost:6379

# JWT
JWT_SECRET=your-jwt-secret
COOKIE_SECRET=your-cookie-secret

# AWS S3
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_REGION=us-east-1
AWS_BUCKET=meca-storage

# PagBank
PAGBANK_CLIENT_ID=your-client-id
PAGBANK_CLIENT_SECRET=your-client-secret

# FCM
FCM_SERVER_KEY=your-fcm-server-key
```

## 📡 Endpoints Principais

### Admin
- `GET /admin/dashboard-metrics` - Métricas do dashboard
- `GET /admin/workshops/pending` - Oficinas pendentes
- `POST /admin/workshops/:id/approve` - Aprovar oficina
- `GET /admin/workshops/ranking` - Ranking de oficinas

### Store (Cliente)
- `GET /store/services` - Listar serviços
- `POST /store/book-service` - Agendar serviço
- `GET /store/my-vehicles` - Meus veículos
- `POST /store/my-vehicles` - Adicionar veículo

### Store (Oficina)
- `POST /store/workshops` - Cadastrar oficina
- `PUT /store/workshops/me` - Atualizar perfil
- `GET /store/bookings` - Meus agendamentos
- `POST /store/bookings/:id/confirm` - Confirmar agendamento

## 🗄️ Estrutura do Banco

### Tabelas Principais
- `customer` - Clientes
- `store` - Oficinas
- `product` - Serviços/Produtos
- `vehicle` - Veículos
- `booking` - Agendamentos
- `order` - Pedidos
- `review` - Avaliações

### Relacionamentos
- Customer → Vehicle (1:N)
- Store → Product (1:N)
- Customer → Booking (1:N)
- Store → Booking (1:N)
- Product → Booking (1:N)
- Vehicle → Booking (1:N)

## 🔧 Módulos Customizados

- **Oficina Module:** Gestão de oficinas
- **Vehicle Module:** Gestão de veículos
- **Booking Module:** Sistema de agendamentos
- **Review Module:** Sistema de avaliações
- **PagBank Payment:** Gateway de pagamento
- **FCM Notification:** Notificações push

## 📊 Métricas e Analytics

- Total de agendamentos
- Receita total
- Comissão MECA
- Performance das oficinas
- Crescimento mensal
- Taxa de conversão

## 🔐 Autenticação

### JWT
```javascript
// Login
POST /auth/customer/token
{
  "email": "user@example.com",
  "password": "password"
}
```

### OAuth
- Google OAuth 2.0
- Apple Sign In

## 💳 Pagamentos

### PagBank Integration
```javascript
// Iniciar pagamento
POST /payments/pagbank/create
{
  "amount": 10000,
  "order_id": "order_123"
}
```

## 🔔 Notificações

### FCM Push
```javascript
// Enviar notificação
POST /notifications/send
{
  "token": "fcm_token",
  "title": "Novo agendamento",
  "body": "Seu agendamento foi confirmado"
}
```

## 🚀 Deploy

### Local
```bash
yarn dev
```

### Produção
```bash
# Build
yarn build

# Com PM2
pm2 start dist/index.js --name meca-api

# Com Docker
docker build -t meca-api .
docker run -p 9000:9000 meca-api
```

## 📝 Licença

© 2024 MECA - Todos os direitos reservados.

## 👥 Equipe

Desenvolvido pela equipe MECA Devs.