import { defineConfig, loadEnv } from '@medusajs/framework/utils'

loadEnv(process.env.NODE_ENV || 'development', process.cwd())

module.exports = defineConfig({
  projectConfig: {
    databaseUrl: process.env.DATABASE_URL,
    http: {
      storeCors: process.env.STORE_CORS || "http://localhost:3000",
      adminCors: process.env.ADMIN_CORS || "http://localhost:7001",
      authCors: process.env.AUTH_CORS || "http://localhost:3000",
      jwtSecret: process.env.JWT_SECRET || "supersecret",
      cookieSecret: process.env.COOKIE_SECRET || "supersecret",
    },
    workerMode: process.env.WORKER_MODE === 'true' ? 'worker' : 'shared',
  },
  admin: {
    disable: true, // Desabilitar admin do Medusa para usar Next.js separado
  },
  modules: [
    {
      resolve: "./src/modules/booking",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/vehicle",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/oficina",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/master_service",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/review",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/service_photo",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/system_alert",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/vehicle_history",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/fcm_notification",
      options: {
        isQueryable: true
      }
    },
    {
      resolve: "./src/modules/pagbank_payment",
      options: {
        isQueryable: true
      }
    }
  ]
})
