/**
 * Endpoint de registro para oficinas
 * POST /auth/workshop/register
 */

import { MedusaRequest, MedusaResponse } from "@medusajs/medusa"
import { AuthService } from "../../../services/auth"
import EmailService from "../../../services/email"

export const AUTHENTICATE = false

export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const { email, password, name, company_name, phone, cnpj } = req.body

  if (!email || !password || !name || !cnpj) {
    return res.status(400).json({
      message: "Email, senha, nome, CNPJ são obrigatórios"
    })
  }

  try {
    // Verificar se oficina já existe
    const oficinaService = req.scope.resolve("oficinaModuleService")
    const existingOficinas = await oficinaService.list({
      email,
      take: 1
    })

    if (existingOficinas.length > 0) {
      return res.status(409).json({
        message: "Oficina já cadastrada com este email"
      })
    }

    // Verificar CNPJ duplicado
    const existingCnpj = await oficinaService.list({
      cnpj,
      take: 1
    })

    if (existingCnpj.length > 0) {
      return res.status(409).json({
        message: "CNPJ já cadastrado"
      })
    }

    // Hash da senha
    const passwordHash = await AuthService.hashPassword(password)

    // Criar oficina
    const oficina = await oficinaService.create({
      name,
      email,
      phone,
      cnpj,
      status: 'pendente', // Aguardando aprovação
      address: {
        street: '',
        city: '',
        state: '',
        zipCode: ''
      },
      metadata: {
        password_hash: passwordHash,
        company_name: company_name,
        created_via: 'api'
      }
    })

    // Enviar email de boas-vindas
    try {
      await EmailService.sendWelcomeWorkshop({
        name,
        email,
        companyName: company_name,
        phone
      })
    } catch (emailError) {
      console.error("Erro ao enviar email de boas-vindas:", emailError)
      // Não falhar o registro por erro de email
    }

    // Gerar token JWT
    const token = AuthService.generateToken({
      id: oficina.id,
      email: oficina.email,
      type: 'workshop'
    })

    return res.status(201).json({
      message: "Oficina cadastrada com sucesso! Aguardando aprovação.",
      access_token: token,
      token_type: "Bearer",
      expires_in: 604800, // 7 dias
      workshop: {
        id: oficina.id,
        name: oficina.name,
        email: oficina.email,
        status: oficina.status,
        company_name: company_name
      }
    })

  } catch (error) {
    console.error("Erro ao cadastrar oficina:", error)

    return res.status(500).json({
      message: "Erro ao cadastrar oficina",
      error: error.message
    })
  }
}
