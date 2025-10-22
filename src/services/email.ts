/**
 * Email Service usando Nodemailer
 * Envia emails transacionais lindos e personalizados (boas-vindas, recuperação de senha, notificações)
 */

const nodemailer = require('nodemailer')

// Configurações SMTP do Gmail
const SMTP_CONFIG = {
  host: process.env.SMTP_HOST || 'smtp.gmail.com',
  port: parseInt(process.env.SMTP_PORT || '587'),
  secure: false, // true para 465, false para outras portas
  auth: {
    user: process.env.SMTP_EMAIL || 'suporte@mecabr.com',
    pass: process.env.SMTP_PASSWORD || 'yqgt ctvh jppd idnd'
  }
}

// URLs públicas (ajustar conforme necessário)
const LOGO_URL = 'https://i.imgur.com/mecabr-logo.png' // TODO: Hospedar logo
const APP_URL = 'http://ec2-3-144-213-137.us-east-2.compute.amazonaws.com:9000'

interface EmailOptions {
  to: string | string[]
  subject: string
  html: string
  text?: string
}

/**
 * Template base para todos os emails
 */
function getBaseTemplate(content: string): string {
  return `
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>MECA - Sua Oficina na Palma da Mão</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { 
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
      line-height: 1.6; 
      color: #1F2937;
      background: #F3F4F6;
      padding: 20px;
    }
    .email-wrapper {
      max-width: 600px;
      margin: 0 auto;
      background: white;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 10px 40px rgba(0,0,0,0.1);
    }
    .header {
      background: linear-gradient(135deg, #22C55E 0%, #16A34A 100%);
      padding: 40px 30px;
      text-align: center;
      position: relative;
    }
    .header::after {
      content: '';
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      height: 20px;
      background: white;
      border-radius: 20px 20px 0 0;
    }
    .logo-container {
      background: white;
      width: 100px;
      height: 100px;
      border-radius: 50%;
      margin: 0 auto 20px;
      display: flex;
      align-items: center;
      justify-content: center;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    }
    .logo {
      font-size: 48px;
      font-weight: bold;
      color: #22C55E;
    }
    .header h1 {
      color: white;
      font-size: 28px;
      margin: 0;
      text-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .content {
      padding: 40px 30px;
    }
    .content p {
      margin-bottom: 16px;
      color: #4B5563;
      font-size: 16px;
    }
    .content strong {
      color: #1F2937;
    }
    .highlight-box {
      background: linear-gradient(135deg, #F0FDF4 0%, #DCFCE7 100%);
      border-left: 4px solid #22C55E;
      border-radius: 8px;
      padding: 20px;
      margin: 24px 0;
    }
    .info-box {
      background: #F9FAFB;
      border: 2px solid #E5E7EB;
      border-radius: 12px;
      padding: 24px;
      margin: 24px 0;
    }
    .info-row {
      display: flex;
      justify-content: space-between;
      padding: 12px 0;
      border-bottom: 1px solid #E5E7EB;
    }
    .info-row:last-child {
      border-bottom: none;
    }
    .info-label {
      font-weight: 600;
      color: #6B7280;
    }
    .info-value {
      color: #1F2937;
      font-weight: 500;
    }
    .button {
      display: inline-block;
      background: linear-gradient(135deg, #22C55E 0%, #16A34A 100%);
      color: white !important;
      padding: 16px 40px;
      text-decoration: none;
      border-radius: 12px;
      font-weight: 600;
      font-size: 16px;
      margin: 24px 0;
      box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
      transition: all 0.3s;
    }
    .button:hover {
      box-shadow: 0 6px 20px rgba(34, 197, 94, 0.4);
      transform: translateY(-2px);
    }
    .button-center {
      text-align: center;
    }
    .code-box {
      background: linear-gradient(135deg, #1F2937 0%, #111827 100%);
      color: #22C55E;
      padding: 20px;
      border-radius: 12px;
      font-family: 'Courier New', monospace;
      font-size: 32px;
      font-weight: bold;
      text-align: center;
      letter-spacing: 8px;
      margin: 24px 0;
      box-shadow: 0 4px 12px rgba(0,0,0,0.2);
    }
    .feature-list {
      list-style: none;
      padding: 0;
    }
    .feature-list li {
      padding: 12px 0;
      padding-left: 36px;
      position: relative;
      color: #4B5563;
    }
    .feature-list li:before {
      content: '✓';
      position: absolute;
      left: 0;
      top: 12px;
      background: #22C55E;
      color: white;
      width: 24px;
      height: 24px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      font-size: 14px;
    }
    .alert-box {
      background: linear-gradient(135deg, #FEF3C7 0%, #FDE68A 100%);
      border-left: 4px solid #F59E0B;
      border-radius: 8px;
      padding: 20px;
      margin: 24px 0;
    }
    .alert-box strong {
      color: #92400E;
    }
    .alert-box p {
      margin-bottom: 0;
      color: #78350F;
    }
    .danger-box {
      background: linear-gradient(135deg, #FEE2E2 0%, #FECACA 100%);
      border-left: 4px solid #EF4444;
      border-radius: 8px;
      padding: 20px;
      margin: 24px 0;
    }
    .danger-box strong {
      color: #991B1B;
    }
    .danger-box p {
      margin-bottom: 0;
      color: #7F1D1D;
    }
    .footer {
      background: #F9FAFB;
      padding: 30px;
      text-align: center;
      border-top: 1px solid #E5E7EB;
    }
    .footer-logo {
      font-size: 24px;
      font-weight: bold;
      color: #22C55E;
      margin-bottom: 12px;
    }
    .footer p {
      color: #6B7280;
      font-size: 14px;
      margin: 8px 0;
    }
    .footer-links {
      margin: 16px 0;
    }
    .footer-links a {
      color: #22C55E;
      text-decoration: none;
      margin: 0 12px;
      font-size: 14px;
    }
    .social-icons {
      margin: 16px 0;
    }
    .social-icon {
      display: inline-block;
      width: 36px;
      height: 36px;
      background: #E5E7EB;
      border-radius: 50%;
      margin: 0 6px;
      line-height: 36px;
      text-decoration: none;
      color: #6B7280;
      font-weight: bold;
    }
    @media only screen and (max-width: 600px) {
      .email-wrapper { border-radius: 0; }
      .header, .content, .footer { padding: 24px 20px; }
      .header h1 { font-size: 24px; }
      .code-box { font-size: 24px; letter-spacing: 4px; }
    }
  </style>
</head>
<body>
  <div class="email-wrapper">
    ${content}
    <div class="footer">
      <div class="footer-logo">🔧 MECA</div>
      <p><strong>Sua Oficina na Palma da Mão</strong></p>
      <p>© ${new Date().getFullYear()} MECA - Todos os direitos reservados</p>
      <div class="footer-links">
        <a href="${APP_URL}">Site</a> •
        <a href="mailto:contato@mecabr.com">Contato</a> •
        <a href="${APP_URL}/termos">Termos</a> •
        <a href="${APP_URL}/privacidade">Privacidade</a>
      </div>
      <p style="margin-top: 16px;">
        📧 <a href="mailto:contato@mecabr.com" style="color: #22C55E;">contato@mecabr.com</a><br>
        📱 WhatsApp: (61) 99999-9999
      </p>
    </div>
  </div>
</body>
</html>
`
}

export class EmailService {
  private static transporter = nodemailer.createTransporter(SMTP_CONFIG)

  /**
   * Verifica conexão SMTP
   */
  static async verifyConnection(): Promise<boolean> {
    try {
      await this.transporter.verify()
      console.log('✅ Conexão SMTP verificada com sucesso')
      return true
    } catch (error) {
      console.error('❌ Erro na conexão SMTP:', error)
      return false
    }
  }

  /**
   * Envia um email
   */
  static async sendEmail(options: EmailOptions): Promise<boolean> {
    try {
      const info = await this.transporter.sendMail({
        from: `"MECA - Sua Oficina na Palma da Mão" <${SMTP_CONFIG.auth.user}>`,
        to: Array.isArray(options.to) ? options.to.join(', ') : options.to,
        subject: options.subject,
        text: options.text || '',
        html: options.html
      })

      console.log('✅ Email enviado:', info.messageId, 'para:', options.to)
      return true
    } catch (error) {
      console.error('❌ Erro ao enviar email:', error)
      return false
    }
  }

  /**
   * Email de boas-vindas para cliente
   */
  static async sendWelcomeCustomer(email: string, name: string): Promise<boolean> {
    const content = `
    <div class="header">
      <div class="logo-container">
        <div class="logo">🔧</div>
      </div>
      <h1>🎉 Bem-vindo ao MECA!</h1>
    </div>
    <div class="content">
      <p>Olá <strong>${name}</strong>,</p>
      
      <div class="highlight-box">
        <p style="margin: 0;"><strong>🎊 Parabéns!</strong> Sua conta foi criada com sucesso. Agora você faz parte da maior plataforma de serviços automotivos do Brasil!</p>
      </div>
      
      <p>Com o <strong>MECA</strong> você tem acesso a:</p>
      
      <ul class="feature-list">
        <li><strong>Oficinas Verificadas</strong> - Encontre as melhores oficinas da sua região</li>
        <li><strong>Agendamento Fácil</strong> - Agende serviços em poucos cliques</li>
        <li><strong>Histórico Completo</strong> - Acompanhe todo o histórico do seu veículo</li>
        <li><strong>Avaliações Reais</strong> - Confira avaliações de outros clientes</li>
        <li><strong>Pagamento Seguro</strong> - Pague direto pelo app com segurança</li>
      </ul>
      
      <div class="button-center">
        <a href="${APP_URL}" class="button">🚀 Começar a Usar Agora</a>
      </div>
      
      <p style="margin-top: 24px;">Se você tiver qualquer dúvida, nossa equipe está sempre pronta para ajudar! Basta responder este email ou entrar em contato pelo WhatsApp.</p>
      
      <p style="font-size: 18px; margin-top: 32px;"><strong>Bem-vindo à família MECA! 🚗💚</strong></p>
      
      <p>Abraços,<br><strong>Equipe MECA</strong></p>
    </div>
`

    return this.sendEmail({
      to: email,
      subject: '🎉 Bem-vindo ao MECA - Sua Oficina na Palma da Mão!',
      html: getBaseTemplate(content),
      text: `Olá ${name}, bem-vindo ao MECA! Sua conta foi criada com sucesso. Comece a usar agora: ${APP_URL}`
    })
  }

  /**
   * Email de boas-vindas para oficina
   */
  static async sendWelcomeWorkshop(email: string, name: string): Promise<boolean> {
    const content = `
    <div class="header">
      <div class="logo-container">
        <div class="logo">🔧</div>
      </div>
      <h1>🏆 Bem-vindo ao MECA Oficina!</h1>
    </div>
    <div class="content">
      <p>Olá <strong>${name}</strong>,</p>
      
      <div class="highlight-box">
        <p style="margin: 0;"><strong>🎊 Parabéns!</strong> Sua oficina foi cadastrada com sucesso na plataforma MECA!</p>
      </div>
      
      <div class="alert-box">
        <p><strong>⏳ Aguardando Aprovação</strong></p>
        <p>Sua conta está em análise pela nossa equipe. Verificaremos seus dados e você receberá um email assim que for aprovada. Este processo geralmente leva até 24 horas.</p>
      </div>
      
      <p><strong>Após a aprovação, você poderá:</strong></p>
      
      <ul class="feature-list">
        <li><strong>Receber Agendamentos</strong> - Clientes próximos encontrarão sua oficina</li>
        <li><strong>Gerenciar Agenda</strong> - Organize seus horários e serviços</li>
        <li><strong>Histórico Completo</strong> - Acompanhe todos os atendimentos</li>
        <li><strong>Receber Pagamentos</strong> - Pagamentos seguros direto na plataforma</li>
        <li><strong>Aumentar Visibilidade</strong> - Seja encontrado por mais clientes</li>
      </ul>
      
      <div class="info-box">
        <h3 style="margin-top: 0; color: #22C55E;">📱 Próximos Passos</h3>
        <p style="margin-bottom: 12px;">Enquanto aguarda a aprovação:</p>
        <ol style="margin-left: 20px; color: #4B5563;">
          <li style="margin-bottom: 8px;">Baixe o aplicativo MECA Oficina</li>
          <li style="margin-bottom: 8px;">Prepare fotos da sua oficina</li>
          <li style="margin-bottom: 8px;">Liste os serviços que você oferece</li>
          <li>Defina seus horários de atendimento</li>
        </ol>
      </div>
      
      <p style="font-size: 18px; margin-top: 32px;"><strong>Estamos ansiosos para tê-lo como parceiro! 🤝</strong></p>
      
      <p>Abraços,<br><strong>Equipe MECA</strong></p>
    </div>
`

    return this.sendEmail({
      to: email,
      subject: '🏆 Bem-vindo ao MECA Oficina - Cadastro Realizado!',
      html: getBaseTemplate(content),
      text: `Olá ${name}, sua oficina foi cadastrada com sucesso no MECA! Aguarde a aprovação da nossa equipe.`
    })
  }

  /**
   * Email de recuperação de senha
   */
  static async sendPasswordReset(email: string, name: string, resetToken: string, userType: 'customer' | 'workshop'): Promise<boolean> {
    const resetUrl = `${APP_URL}/auth/${userType}/reset-password?token=${resetToken}`
    
    const content = `
    <div class="header">
      <div class="logo-container">
        <div class="logo">🔧</div>
      </div>
      <h1>🔐 Recuperação de Senha</h1>
    </div>
    <div class="content">
      <p>Olá <strong>${name}</strong>,</p>
      
      <p>Recebemos uma solicitação para redefinir a senha da sua conta <strong>MECA</strong>.</p>
      
      <div class="danger-box">
        <p><strong>⚠️ Segurança em Primeiro Lugar</strong></p>
        <p>Se você <strong>NÃO</strong> solicitou a redefinição de senha, <strong>ignore este email</strong>. Sua conta permanecerá segura e nenhuma alteração será feita.</p>
      </div>
      
      <p><strong>Para criar uma nova senha, use o código abaixo:</strong></p>
      
      <div class="code-box">${resetToken}</div>
      
      <p style="text-align: center; color: #EF4444; font-weight: 600;">⏰ Este código expira em 1 hora</p>
      
      <div class="button-center">
        <a href="${resetUrl}" class="button">🔑 Redefinir Senha Agora</a>
      </div>
      
      <div class="info-box">
        <h3 style="margin-top: 0; color: #6B7280;">📱 Como usar no app:</h3>
        <ol style="margin-left: 20px; color: #4B5563;">
          <li style="margin-bottom: 8px;">Abra o app MECA</li>
          <li style="margin-bottom: 8px;">Vá em "Esqueci minha senha"</li>
          <li style="margin-bottom: 8px;">Digite o código: <strong>${resetToken}</strong></li>
          <li>Crie sua nova senha</li>
        </ol>
      </div>
      
      <p>Se você tiver problemas ou não solicitou esta redefinição, entre em contato conosco imediatamente.</p>
      
      <p>Abraços,<br><strong>Equipe MECA</strong></p>
    </div>
`

    return this.sendEmail({
      to: email,
      subject: '🔐 Recuperação de Senha - MECA',
      html: getBaseTemplate(content),
      text: `Olá ${name}, use este código para redefinir sua senha no MECA: ${resetToken}. O código expira em 1 hora.`
    })
  }

  /**
   * Email de agendamento confirmado
   */
  static async sendBookingConfirmed(
    customerEmail: string,
    customerName: string,
    workshopName: string,
    serviceTitle: string,
    scheduledDate: Date
  ): Promise<boolean> {
    const dateStr = scheduledDate.toLocaleDateString('pt-BR', { 
      weekday: 'long',
      day: '2-digit', 
      month: 'long', 
      year: 'numeric'
    })
    
    const timeStr = scheduledDate.toLocaleTimeString('pt-BR', {
      hour: '2-digit',
      minute: '2-digit'
    })
    
    const content = `
    <div class="header">
      <div class="logo-container">
        <div class="logo">🔧</div>
      </div>
      <h1>✅ Agendamento Confirmado!</h1>
    </div>
    <div class="content">
      <p>Olá <strong>${customerName}</strong>,</p>
      
      <div class="highlight-box">
        <p style="margin: 0; font-size: 18px;"><strong>🎉 Ótimas notícias!</strong> Seu agendamento foi <strong style="color: #22C55E;">confirmado</strong> pela oficina!</p>
      </div>
      
      <div class="info-box">
        <h3 style="margin-top: 0; color: #22C55E; text-align: center;">📋 Detalhes do Agendamento</h3>
        <div class="info-row">
          <span class="info-label">🏢 Oficina:</span>
          <span class="info-value">${workshopName}</span>
        </div>
        <div class="info-row">
          <span class="info-label">🔧 Serviço:</span>
          <span class="info-value">${serviceTitle}</span>
        </div>
        <div class="info-row">
          <span class="info-label">📅 Data:</span>
          <span class="info-value">${dateStr}</span>
        </div>
        <div class="info-row">
          <span class="info-label">🕐 Horário:</span>
          <span class="info-value">${timeStr}</span>
        </div>
      </div>
      
      <div class="alert-box">
        <p><strong>⏰ Dicas Importantes:</strong></p>
        <ul style="margin: 12px 0 0 20px; color: #78350F;">
          <li>Chegue com 10 minutos de antecedência</li>
          <li>Leve os documentos do veículo</li>
          <li>Confira o endereço da oficina no app</li>
        </ul>
      </div>
      
      <p>Se precisar <strong>cancelar ou remarcar</strong>, faça isso com antecedência pelo aplicativo para não prejudicar a oficina.</p>
      
      <div class="button-center">
        <a href="${APP_URL}" class="button">📱 Abrir no App</a>
      </div>
      
      <p style="font-size: 18px; margin-top: 32px; text-align: center;"><strong>Bom atendimento! 🚗✨</strong></p>
      
      <p>Abraços,<br><strong>Equipe MECA</strong></p>
    </div>
`

    return this.sendEmail({
      to: customerEmail,
      subject: `✅ Agendamento Confirmado - ${workshopName}`,
      html: getBaseTemplate(content),
      text: `Olá ${customerName}, seu agendamento em ${workshopName} para ${serviceTitle} foi confirmado para ${dateStr} às ${timeStr}.`
    })
  }

  /**
   * Email de agendamento recusado
   */
  static async sendBookingRejected(
    customerEmail: string,
    customerName: string,
    workshopName: string,
    serviceTitle: string,
    reason?: string
  ): Promise<boolean> {
    const content = `
    <div class="header" style="background: linear-gradient(135deg, #EF4444 0%, #DC2626 100%);">
      <div class="logo-container">
        <div class="logo" style="color: #EF4444;">🔧</div>
      </div>
      <h1>😔 Agendamento Não Confirmado</h1>
    </div>
    <div class="content">
      <p>Olá <strong>${customerName}</strong>,</p>
      
      <div class="danger-box">
        <p style="margin: 0; font-size: 16px;"><strong>Infelizmente</strong>, a oficina <strong>${workshopName}</strong> não pôde confirmar seu agendamento para <strong>${serviceTitle}</strong>.</p>
      </div>
      
      ${reason ? `
      <div class="info-box">
        <h3 style="margin-top: 0; color: #EF4444;">📝 Motivo informado pela oficina:</h3>
        <p style="margin-bottom: 0; font-style: italic; color: #1F2937;">"${reason}"</p>
      </div>
      ` : ''}
      
      <p><strong>Mas não se preocupe!</strong> Você tem outras opções:</p>
      
      <ul class="feature-list">
        <li>Tentar agendar em outro horário nesta mesma oficina</li>
        <li>Buscar outras oficinas disponíveis na sua região</li>
        <li>Entrar em contato direto com a oficina para negociar</li>
        <li>Verificar oficinas com avaliações excelentes perto de você</li>
      </ul>
      
      <div class="button-center">
        <a href="${APP_URL}" class="button" style="background: linear-gradient(135deg, #22C55E 0%, #16A34A 100%);">🔍 Buscar Outras Oficinas</a>
      </div>
      
      <p style="margin-top: 24px;">Temos certeza que você encontrará a oficina perfeita para suas necessidades!</p>
      
      <p>Se precisar de ajuda, estamos aqui! 💚</p>
      
      <p>Abraços,<br><strong>Equipe MECA</strong></p>
    </div>
`

    return this.sendEmail({
      to: customerEmail,
      subject: `😔 Agendamento Não Confirmado - ${workshopName}`,
      html: getBaseTemplate(content),
      text: `Olá ${customerName}, infelizmente seu agendamento em ${workshopName} não foi confirmado. ${reason ? `Motivo: ${reason}. ` : ''}Busque outras oficinas em: ${APP_URL}`
    })
  }

  /**
   * Email de oficina aprovada
   */
  static async sendWorkshopApproved(email: string, name: string): Promise<boolean> {
    const content = `
    <div class="header">
      <div class="logo-container">
        <div class="logo">🔧</div>
      </div>
      <h1>🎉 Oficina Aprovada!</h1>
    </div>
    <div class="content">
      <p>Olá <strong>${name}</strong>,</p>
      
      <div class="highlight-box">
        <p style="margin: 0; font-size: 20px; text-align: center;">
          <strong>🎊 PARABÉNS! 🎊</strong><br>
          <span style="font-size: 18px; margin-top: 12px; display: block;">Sua oficina foi <strong style="color: #22C55E;">APROVADA</strong> e agora está <strong style="color: #22C55E;">ATIVA</strong> na plataforma MECA!</span>
        </p>
      </div>
      
      <p style="font-size: 18px; text-align: center; margin: 32px 0;"><strong>🚀 Você já pode começar a receber clientes!</strong></p>
      
      <div class="info-box">
        <h3 style="margin-top: 0; color: #22C55E; text-align: center;">✨ O que você já pode fazer:</h3>
        <ul class="feature-list">
          <li><strong>Receber Agendamentos</strong> - Clientes já podem encontrar sua oficina</li>
          <li><strong>Gerenciar Agenda</strong> - Configure seus horários e disponibilidade</li>
          <li><strong>Cadastrar Serviços</strong> - Adicione serviços e defina preços</li>
          <li><strong>Receber Pagamentos</strong> - Sistema seguro de pagamentos integrado</li>
          <li><strong>Aumentar Faturamento</strong> - Alcance mais clientes na sua região</li>
        </ul>
      </div>
      
      <div class="button-center">
        <a href="${APP_URL}" class="button">🚀 Acessar Painel Agora</a>
      </div>
      
      <div class="alert-box">
        <p><strong>💡 Dica de Sucesso:</strong></p>
        <p>Complete seu perfil com fotos profissionais, liste todos os seus serviços e mantenha seus horários sempre atualizados. Oficinas com perfis completos recebem até 3x mais agendamentos!</p>
      </div>
      
      <p style="font-size: 20px; margin-top: 32px; text-align: center;"><strong>Bem-vindo à família MECA! 🤝💚</strong></p>
      
      <p style="text-align: center;">Estamos felizes em tê-lo como parceiro.<br><strong>Vamos crescer juntos! 🚀</strong></p>
      
      <p>Abraços,<br><strong>Equipe MECA</strong></p>
    </div>
`

    return this.sendEmail({
      to: email,
      subject: '🎉 Parabéns! Sua Oficina Foi Aprovada - MECA',
      html: getBaseTemplate(content),
      text: `Olá ${name}, parabéns! Sua oficina foi aprovada e já está ativa na plataforma MECA. Acesse: ${APP_URL}`
    })
  }
}
