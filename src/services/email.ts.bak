const nodemailer = require('nodemailer')

const SMTP_CONFIG = {
  host: process.env.SMTP_HOST || 'smtp.gmail.com',
  port: parseInt(process.env.SMTP_PORT || '587'),
  secure: false,
  auth: {
    user: process.env.SMTP_EMAIL || 'suporte@mecabr.com',
    pass: process.env.SMTP_PASSWORD || 'yqgt ctvh jppd idnd'
  }
}

const transporter = nodemailer.createTransporter(SMTP_CONFIG)

export class EmailService {
  static async sendEmail(to: string, subject: string, html: string) {
    try {
      await transporter.sendMail({
        from: `MECA <${SMTP_CONFIG.auth.user}>`,
        to,
        subject,
        html
      })
      console.log(`Email sent to ${to}`)
    } catch (error) {
      console.error(`Error sending email:`, error)
      throw error
    }
  }

  static async sendWelcomeCustomer(to: string, name: string) {
    const subject = 'Bem-vindo à MECA!'
    const html = `<h1>Olá, ${name}!</h1><p>Bem-vindo à plataforma MECA!</p>`
    await this.sendEmail(to, subject, html)
  }

  static async sendPasswordReset(to: string, name: string, token: string) {
    const resetLink = `http://ec2-3-144-213-137.us-east-2.compute.amazonaws.com:9000/reset-password?token=${token}`
    const subject = 'Redefinição de Senha - MECA'
    const html = `<h1>Olá, ${name}!</h1><p>Clique no link para redefinir sua senha:</p><a href="${resetLink}">Redefinir Senha</a>`
    await this.sendEmail(to, subject, html)
  }
}
