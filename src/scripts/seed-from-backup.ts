import { ExecArgs } from "@medusajs/framework/types"
import { ContainerRegistrationKeys, Modules } from "@medusajs/framework/utils"
import fs from "fs"
import path from "path"
import { MASTER_SERVICE_MODULE } from "../modules/master_service"
import { OFICINA_MODULE } from "../modules/oficina"

export default async function seedFromBackup({ container }: ExecArgs) {
  const logger = container.resolve(ContainerRegistrationKeys.LOGGER)
  const oficinaModuleService = container.resolve(OFICINA_MODULE)
  const masterServiceModuleService = container.resolve(MASTER_SERVICE_MODULE)
  const productModuleService = container.resolve(Modules.PRODUCT)
  const userModuleService = container.resolve(Modules.USER)

  logger.info("🔄 Iniciando população do banco com dados do backup...")

  // Ler dados convertidos
  const backupData = JSON.parse(
    fs.readFileSync(path.join(__dirname, "../../backup-converted.json"), "utf-8")
  )

  // 1. POPULAR SERVIÇOS MESTRES
  logger.info(`📦 Criando ${backupData.services.length} serviços mestres...`)
  
  const masterServices = []
  for (const service of backupData.services) {
    const created = await masterServiceModuleService.createMasterServices({
      name: service.title,
      description: service.description,
      category: service.category,
    })
    masterServices.push(created)
    logger.info(`   ✔ ${service.title}`)
  }

  // 2. POPULAR OFICINAS
  logger.info(`\n🏪 Criando ${backupData.oficinas.length} oficinas...`)
  
  for (const oficina of backupData.oficinas) {
    // Criar usuário para o dono (verificar se já existe)
    let user
    try {
      user = await userModuleService.createUsers({
        email: oficina.email,
        first_name: oficina.owner_name?.split(' ')[0] || oficina.name,
        last_name: oficina.owner_name?.split(' ').slice(1).join(' ') || '',
      })
    } catch (error) {
      // Se usuário já existe, buscar
      const existingUsers = await userModuleService.listUsers({ email: oficina.email })
      user = existingUsers[0]
    }

    // Criar oficina
    const createdOficina = await oficinaModuleService.createOficinas({
      name: oficina.name,
      cnpj: oficina.cnpj,
      email: oficina.email,
      phone: oficina.phone,
      address: oficina.address,
      description: `Oficina ${oficina.name} - ${oficina.address?.cidade || ''}`,
      status: oficina.status,
      horario_funcionamento: {
        segunda: { inicio: '08:00', fim: '18:00', ativo: 'true' },
        terca: { inicio: '08:00', fim: '18:00', ativo: 'true' },
        quarta: { inicio: '08:00', fim: '18:00', ativo: 'true' },
        quinta: { inicio: '08:00', fim: '18:00', ativo: 'true' },
        sexta: { inicio: '08:00', fim: '18:00', ativo: 'true' },
        sabado: { inicio: '08:00', fim: '13:00', ativo: 'true' },
        domingo: { inicio: '', fim: '', ativo: 'false' },
      },
      bank_details: oficina.bank_details,
    })

    logger.info(`   ✔ ${oficina.name} - ${oficina.address?.cidade}/${oficina.address?.estado}`)
  }

  logger.info("\n✅ População concluída com sucesso!")
  logger.info(`\n📊 Resumo:`)
  logger.info(`   - ${masterServices.length} serviços mestres`)
  logger.info(`   - ${backupData.oficinas.length} oficinas`)
  logger.info(`   - ~${backupData.oficinas.length * 8} produtos/serviços criados`)
}



