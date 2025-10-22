/**
 * Serviço de Email da MECA
 * Implementa templates profissionais com identidade visual da MECA
 */

const nodemailer = require('nodemailer')

// Configuração SMTP
const SMTP_CONFIG = {
  host: 'smtp.gmail.com',
  port: 587,
  secure: false,
  auth: {
    user: process.env.EMAIL_USER || 'meca@mecabr.com',
    pass: process.env.EMAIL_PASS || 'sua_senha_aqui'
  }
}

const transporter = nodemailer.createTransporter(SMTP_CONFIG)

/**
 * Template base para todos os emails da MECA
 */
function getBaseTemplate(content: string, title: string): string {
  return `
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>${title}</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f8f9fa;
            margin: 0;
            padding: 0;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        .header {
            background: linear-gradient(135deg, #00A651 0%, #007A3D 100%);
            color: white;
            padding: 30px 20px;
            text-align: center;
        }
        .logo {
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 10px;
        }
        .tagline {
            font-size: 14px;
            opacity: 0.9;
        }
        .content {
            padding: 40px 30px;
        }
        .footer {
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            border-top: 1px solid #e9ecef;
        }
        .footer-text {
            font-size: 12px;
            color: #6c757d;
            margin-bottom: 10px;
        }
        .social-links {
            margin-top: 15px;
        }
        .social-links a {
            color: #00A651;
            text-decoration: none;
            margin: 0 10px;
        }
        .button {
            display: inline-block;
            background-color: #00A651;
            color: white;
            padding: 12px 24px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
        }
        .button:hover {
            background-color: #007A3D;
        }
        .highlight {
            background-color: #e8f5e8;
            padding: 15px;
            border-left: 4px solid #00A651;
            margin: 20px 0;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <div class="logo">MECA</div>
            <div class="tagline">Sua oficina de confiança</div>
        </div>
        
        <div class="content">
            ${content}
        </div>
        
        <div class="footer">
            <div class="footer-text">
                <strong>MECA - Sistema de Agendamento Automotivo</strong><br>
                Conectando clientes e oficinas de forma inteligente
            </div>
            <div class="social-links">
                <a href="https://mecabr.com">Website</a> |
                <a href="mailto:contato@mecabr.com">Contato</a> |
                <a href="https://mecabr.com/suporte">Suporte</a>
            </div>
        </div>
    </div>
</body>
</html>
  `
}

/**
 * Email de boas-vindas para clientes
 */
export async function sendWelcomeCustomer(customer: {
  name: string
  email: string
  phone?: string
}): Promise<void> {
  const content = `
    <h2>Bem-vindo à MECA, ${customer.name}!</h2>
    
    <p>É um prazer tê-lo conosco! Sua conta foi criada com sucesso e você já pode começar a agendar serviços para seu veículo.</p>
    
    <div class="highlight">
        <strong>O que você pode fazer agora:</strong>
        <ul>
            <li>📅 Agendar serviços para seu veículo</li>
            <li>🔍 Encontrar oficinas próximas</li>
            <li>📱 Receber notificações sobre seus agendamentos</li>
            <li>⭐ Avaliar o atendimento recebido</li>
        </ul>
    </div>
    
    <p>Se você tiver alguma dúvida, nossa equipe de suporte está sempre disponível para ajudar.</p>
    
    <a href="https://mecabr.com/app" class="button">Acessar App</a>
    
    <p><strong>Dados da sua conta:</strong></p>
    <ul>
        <li>Nome: ${customer.name}</li>
        <li>Email: ${customer.email}</li>
        ${customer.phone ? `<li>Telefone: ${customer.phone}</li>` : ''}
    </ul>
  `
  
  const mailOptions = {
    from: '"MECA" <noreply@mecabr.com>',
    to: customer.email,
    subject: 'Bem-vindo à MECA! Sua conta foi criada com sucesso',
    html: getBaseTemplate(content, 'Bem-vindo à MECA')
  }
  
  await transporter.sendMail(mailOptions)
}

/**
 * Email de boas-vindas para oficinas
 */
export async function sendWelcomeWorkshop(workshop: {
  name: string
  email: string
  companyName?: string
  phone?: string
}): Promise<void> {
  const content = `
    <h2>Bem-vindo à MECA, ${workshop.name}!</h2>
    
    <p>Sua oficina <strong>${workshop.companyName || 'Oficina'}</strong> foi cadastrada com sucesso em nossa plataforma!</p>
    
    <div class="highlight">
        <strong>O que você pode fazer agora:</strong>
        <ul>
            <li>📅 Gerenciar agendamentos de clientes</li>
            <li>💰 Acompanhar pagamentos e comissões</li>
            <li>📊 Visualizar relatórios de performance</li>
            <li>⭐ Receber avaliações dos clientes</li>
        </ul>
    </div>
    
    <p>Sua conta está sendo analisada por nossa equipe e você receberá uma confirmação de aprovação em breve.</p>
    
    <a href="https://mecabr.com/oficina" class="button">Acessar Painel da Oficina</a>
    
    <p><strong>Dados da sua oficina:</strong></p>
    <ul>
        <li>Responsável: ${workshop.name}</li>
        <li>Email: ${workshop.email}</li>
        ${workshop.companyName ? `<li>Oficina: ${workshop.companyName}</li>` : ''}
        ${workshop.phone ? `<li>Telefone: ${workshop.phone}</li>` : ''}
    </ul>
  `
  
  const mailOptions = {
    from: '"MECA" <noreply@mecabr.com>',
    to: workshop.email,
    subject: 'Bem-vindo à MECA! Sua oficina foi cadastrada',
    html: getBaseTemplate(content, 'Bem-vindo à MECA')
  }
  
  await transporter.sendMail(mailOptions)
}

/**
 * Email de recuperação de senha
 */
export async function sendPasswordReset(email: string, resetToken: string, userType: 'customer' | 'workshop'): Promise<void> {
  const resetUrl = `https://mecabr.com/${userType}/reset-password?token=${resetToken}`
  
  const content = `
    <h2>Recuperação de Senha</h2>
    
    <p>Recebemos uma solicitação para redefinir a senha da sua conta.</p>
    
    <div class="highlight">
        <strong>Para redefinir sua senha:</strong>
        <ol>
            <li>Clique no botão abaixo</li>
            <li>Digite sua nova senha</li>
            <li>Confirme a nova senha</li>
        </ol>
    </div>
    
    <a href="${resetUrl}" class="button">Redefinir Senha</a>
    
    <p><strong>Link direto:</strong><br>
    <a href="${resetUrl}">${resetUrl}</a></p>
    
    <p><em>Este link expira em 1 hora por motivos de segurança.</em></p>
    
    <p>Se você não solicitou esta redefinição, ignore este email.</p>
  `
  
  const mailOptions = {
    from: '"MECA" <noreply@mecabr.com>',
    to: email,
    subject: 'Redefinir senha - MECA',
    html: getBaseTemplate(content, 'Recuperação de Senha')
  }
  
  await transporter.sendMail(mailOptions)
}

/**
 * Email de confirmação de agendamento
 */
export async function sendBookingConfirmed(booking: {
  customerName: string
  customerEmail: string
  workshopName: string
  serviceDate: string
  serviceTime: string
  vehicleInfo: string
  serviceDescription: string
}): Promise<void> {
  const content = `
    <h2>Agendamento Confirmado! 🎉</h2>
    
    <p>Olá <strong>${booking.customerName}</strong>,</p>
    
    <p>Seu agendamento foi <strong>confirmado</strong> pela oficina <strong>${booking.workshopName}</strong>!</p>
    
    <div class="highlight">
        <strong>Detalhes do seu agendamento:</strong>
        <ul>
            <li>📅 <strong>Data:</strong> ${booking.serviceDate}</li>
            <li>🕐 <strong>Horário:</strong> ${booking.serviceTime}</li>
            <li>🚗 <strong>Veículo:</strong> ${booking.vehicleInfo}</li>
            <li>🔧 <strong>Serviço:</strong> ${booking.serviceDescription}</li>
            <li>🏢 <strong>Oficina:</strong> ${booking.workshopName}</li>
        </ul>
    </div>
    
    <p>Chegue com alguns minutos de antecedência e leve a documentação do veículo.</p>
    
    <a href="https://mecabr.com/app/agendamentos" class="button">Ver Meus Agendamentos</a>
    
    <p>Em caso de dúvidas, entre em contato conosco ou com a oficina diretamente.</p>
  `
  
  const mailOptions = {
    from: '"MECA" <noreply@mecabr.com>',
    to: booking.customerEmail,
    subject: `Agendamento confirmado - ${booking.workshopName}`,
    html: getBaseTemplate(content, 'Agendamento Confirmado')
  }
  
  await transporter.sendMail(mailOptions)
}

/**
 * Email de rejeição de agendamento
 */
export async function sendBookingRejected(booking: {
  customerName: string
  customerEmail: string
  workshopName: string
  serviceDate: string
  reason?: string
}): Promise<void> {
  const content = `
    <h2>Agendamento Não Disponível</h2>
    
    <p>Olá <strong>${booking.customerName}</strong>,</p>
    
    <p>Infelizmente, a oficina <strong>${booking.workshopName}</strong> não pôde confirmar seu agendamento para <strong>${booking.serviceDate}</strong>.</p>
    
    ${booking.reason ? `
    <div class="highlight">
        <strong>Motivo:</strong> ${booking.reason}
    </div>
    ` : ''}
    
    <p>Não se preocupe! Você pode:</p>
    <ul>
        <li>🔄 Tentar agendar em outro horário</li>
        <li>🏢 Procurar outras oficinas próximas</li>
        <li>📞 Entrar em contato conosco para ajuda</li>
    </ul>
    
    <a href="https://mecabr.com/app/agendar" class="button">Agendar Novamente</a>
    
    <p>Nossa equipe está sempre disponível para ajudar você a encontrar a melhor solução!</p>
  `
  
  const mailOptions = {
    from: '"MECA" <noreply@mecabr.com>',
    to: booking.customerEmail,
    subject: `Agendamento não disponível - ${booking.workshopName}`,
    html: getBaseTemplate(content, 'Agendamento Não Disponível')
  }
  
  await transporter.sendMail(mailOptions)
}

/**
 * Email de aprovação de oficina
 */
export async function sendWorkshopApproved(workshop: {
  name: string
  email: string
  companyName: string
}): Promise<void> {
  const content = `
    <h2>Parabéns! Sua oficina foi aprovada! 🎉</h2>
    
    <p>Olá <strong>${workshop.name}</strong>,</p>
    
    <p>É com grande satisfação que informamos que sua oficina <strong>${workshop.companyName}</strong> foi <strong>aprovada</strong> em nossa plataforma!</p>
    
    <div class="highlight">
        <strong>O que isso significa:</strong>
        <ul>
            <li>✅ Sua oficina está ativa na plataforma</li>
            <li>📅 Você pode receber agendamentos de clientes</li>
            <li>💰 Começar a gerar receita através da MECA</li>
            <li>📊 Acessar relatórios e métricas</li>
        </ul>
    </div>
    
    <a href="https://mecabr.com/oficina" class="button">Acessar Painel da Oficina</a>
    
    <p>Bem-vindo à nossa rede de oficinas parceiras! Estamos aqui para apoiar o crescimento do seu negócio.</p>
  `
  
  const mailOptions = {
    from: '"MECA" <noreply@mecabr.com>',
    to: workshop.email,
    subject: 'Oficina aprovada - Bem-vindo à MECA!',
    html: getBaseTemplate(content, 'Oficina Aprovada')
  }
  
  await transporter.sendMail(mailOptions)
}

export default {
  sendWelcomeCustomer,
  sendWelcomeWorkshop,
  sendPasswordReset,
  sendBookingConfirmed,
  sendBookingRejected,
  sendWorkshopApproved
}
