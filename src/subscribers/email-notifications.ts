import { SubscriberArgs, SubscriberConfig } from "@medusajs/framework"
import { Modules } from "@medusajs/framework/utils"
import { EmailService } from "../services/email"

/**
 * Subscriber: Email Notifications
 * 
 * Envia emails automaticamente quando eventos importantes acontecem
 * - customer.created: Email de boas-vindas
 * - booking.confirmed: Email de confirmação de agendamento
 * - booking.rejected: Email de rejeição de agendamento
 */

export default async function emailNotificationsHandler({
  event,
  container,
}: SubscriberArgs<any>) {
  const { data, name: event_name } = event
  const logger = container.resolve("logger")
  
  try {
    switch (event_name) {
      case "customer.created":
        await handleCustomerCreated(data, container, logger)
        break
        
      case "booking.confirmed":
        await handleBookingConfirmed(data, container, logger)
        break
        
      case "booking.rejected":
        await handleBookingRejected(data, container, logger)
        break
        
      case "workshop.approved":
        await handleWorkshopApproved(data, container, logger)
        break
        
      default:
        logger.debug(`Email subscriber: Unhandled event ${event_name}`)
    }
  } catch (error) {
    logger.error(`Email subscriber error for ${event_name}:`, error)
    // Não lança erro para não quebrar o fluxo principal
  }
}

async function handleCustomerCreated(data: any, container: any, logger: any) {
  try {
    const customerService = container.resolve(Modules.CUSTOMER)
    const customer = await customerService.retrieveCustomer(data.id)
    
    if (customer && customer.email) {
      await EmailService.sendWelcomeCustomer(
        customer.email,
        customer.first_name || 'Cliente'
      )
      logger.info(`Welcome email sent to customer: ${customer.email}`)
    }
  } catch (error) {
    logger.error(`Failed to send welcome email to customer ${data.id}:`, error)
  }
}

async function handleBookingConfirmed(data: any, container: any, logger: any) {
  try {
    const customerService = container.resolve(Modules.CUSTOMER)
    const customer = await customerService.retrieveCustomer(data.customer_id)
    
    if (customer && customer.email && data.workshop_name && data.service_title && data.scheduled_at) {
      await EmailService.sendBookingConfirmed(
        customer.email,
        customer.first_name || 'Cliente',
        data.workshop_name,
        data.service_title,
        new Date(data.scheduled_at)
      )
      logger.info(`Booking confirmed email sent to: ${customer.email}`)
    }
  } catch (error) {
    logger.error(`Failed to send booking confirmed email:`, error)
  }
}

async function handleBookingRejected(data: any, container: any, logger: any) {
  try {
    const customerService = container.resolve(Modules.CUSTOMER)
    const customer = await customerService.retrieveCustomer(data.customer_id)
    
    if (customer && customer.email && data.workshop_name && data.service_title) {
      await EmailService.sendBookingRejected(
        customer.email,
        customer.first_name || 'Cliente',
        data.workshop_name,
        data.service_title,
        data.reason
      )
      logger.info(`Booking rejected email sent to: ${customer.email}`)
    }
  } catch (error) {
    logger.error(`Failed to send booking rejected email:`, error)
  }
}

async function handleWorkshopApproved(data: any, container: any, logger: any) {
  try {
    if (data.email && data.name) {
      await EmailService.sendWorkshopApproved(data.email, data.name)
      logger.info(`Workshop approved email sent to: ${data.email}`)
    }
  } catch (error) {
    logger.error(`Failed to send workshop approved email:`, error)
  }
}

export const config: SubscriberConfig = {
  event: [
    "customer.created",
    "booking.confirmed",
    "booking.rejected",
    "workshop.approved"
  ],
}

