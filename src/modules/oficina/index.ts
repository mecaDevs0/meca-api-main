import OficinaModuleService from "./service"
import { Module } from "@medusajs/framework/utils"
import Oficina from "./models/oficina"

/**
 * Definição do Módulo Oficina
 * 
 * Este módulo gerencia todas as oficinas parceiras do marketplace MECA
 */

export const OFICINA_MODULE = "oficina"

const oficinaModule = Module(OFICINA_MODULE, {
  service: OficinaModuleService,
})

export default oficinaModule

// Exportar linkable para uso em Module Links
export const linkable = {
  oficina: {
    serviceName: OFICINA_MODULE,
    primaryKey: "id",
    model: Oficina,
  },
}

export * from "./models/oficina"

