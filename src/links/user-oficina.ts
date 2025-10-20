import { defineLink } from "@medusajs/framework/utils"
import UserModule from "@medusajs/medusa/user"
import OficinaModule from "../modules/oficina"

/**
 * Link: User -> Oficina
 * 
 * Vincula um usuário (dono) a uma oficina.
 * Permite que o sistema autentique o dono da oficina e acesse seus dados.
 */

export default defineLink(
  UserModule.linkable.user,
  OficinaModule.linkable.oficina
)



