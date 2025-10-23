# Multi-stage build para otimizar tamanho
FROM node:20-alpine AS base

# Instalar dependências do sistema
RUN apk add --no-cache \
    python3 \
    make \
    g++ \
    libc6-compat

WORKDIR /app

# Copiar package files
COPY package*.json ./

# Stage para dependencies
FROM base AS deps
RUN npm ci --only=production

# Stage final - production  
FROM base AS runner

# Instalar dependências runtime
RUN apk add --no-cache \
    dumb-init \
    curl

WORKDIR /app

ENV NODE_ENV=production
ENV PORT=9000

# Criar usuário não-root
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 medusa

# Copiar node_modules de deps
COPY --from=deps --chown=medusa:nodejs /app/node_modules ./node_modules

# Copiar todos arquivos necessários
COPY --chown=medusa:nodejs ./package*.json ./
COPY --chown=medusa:nodejs ./tsconfig.json ./
COPY --chown=medusa:nodejs ./medusa-config.ts ./
COPY --chown=medusa:nodejs ./instrumentation.ts ./
COPY --chown=medusa:nodejs ./src ./src

USER medusa

EXPOSE 9000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:9000/health/live || exit 1

# Usar dumb-init para gerenciar processos corretamente
ENTRYPOINT ["dumb-init", "--"]

CMD ["npm", "run", "dev"]



# Instalar dependências do sistema
RUN apk add --no-cache \
    python3 \
    make \
    g++ \
    libc6-compat

WORKDIR /app

# Copiar package files
COPY package*.json ./

# Stage para dependencies
FROM base AS deps
RUN npm ci --only=production

# Stage final - production  
FROM base AS runner

# Instalar dependências runtime
RUN apk add --no-cache \
    dumb-init \
    curl

WORKDIR /app

ENV NODE_ENV=production
ENV PORT=9000

# Criar usuário não-root
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 medusa

# Copiar node_modules de deps
COPY --from=deps --chown=medusa:nodejs /app/node_modules ./node_modules

# Copiar todos arquivos necessários
COPY --chown=medusa:nodejs ./package*.json ./
COPY --chown=medusa:nodejs ./tsconfig.json ./
COPY --chown=medusa:nodejs ./medusa-config.ts ./
COPY --chown=medusa:nodejs ./instrumentation.ts ./
COPY --chown=medusa:nodejs ./src ./src

USER medusa

EXPOSE 9000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:9000/health/live || exit 1

# Usar dumb-init para gerenciar processos corretamente
ENTRYPOINT ["dumb-init", "--"]

CMD ["npm", "run", "dev"]



