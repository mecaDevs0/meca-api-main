/**
 * Script para adicionar senhas criptografadas às oficinas existentes
 * Usa bcrypt para criptografar as senhas REAIS
 */

import bcrypt from "bcrypt"
import { OFICINA_MODULE } from "../modules/oficina"

const SALT_ROUNDS = 10
const DEFAULT_PASSWORD = "meca123" // Senha padrão para todas as oficinas do backup

export default async function addPasswordsToWorkshops({
  container
}: any) {
  const logger = container.resolve("logger")
  const oficinaModuleService = container.resolve(OFICINA_MODULE)

  logger.info("🔐 Adicionando senhas criptografadas às oficinas...")

  try {
    // Buscar todas as oficinas
    const oficinas = await oficinaModuleService.listOficinas({})

    logger.info(`📋 Encontradas ${oficinas.length} oficinas`)

    let updated = 0
    let skipped = 0

    for (const oficina of oficinas) {
      // Verificar se já tem senha
      if (oficina.metadata?.password_hash) {
        skipped++
        continue
      }

      // Criptografar senha padrão
      const hashedPassword = await bcrypt.hash(DEFAULT_PASSWORD, SALT_ROUNDS)

      // Atualizar oficina com senha criptografada
      await oficinaModuleService.updateOficinas([{
        id: oficina.id,
        metadata: {
          ...oficina.metadata,
          password_hash: hashedPassword
        }
      }])

      updated++
      logger.info(`   ✔ ${oficina.name} - senha adicionada`)
    }

    logger.info(`✅ Concluído!`)
    logger.info(`   📊 Total: ${oficinas.length}`)
    logger.info(`   ✔ Atualizadas: ${updated}`)
    logger.info(`   ⏭  Puladas (já tinham senha): ${skipped}`)
    logger.info(`\n🔑 Senha padrão para todas as oficinas: ${DEFAULT_PASSWORD}`)
    logger.info(`   Use esta senha para fazer login no app de oficina`)

  } catch (error) {
    logger.error("❌ Erro ao adicionar senhas:", error)
    throw error
  }
}
