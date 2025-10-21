import { createWorkflow, WorkflowResponse } from "@medusajs/framework/workflows"
import { Modules } from "@medusajs/framework/utils"
import { BOOKING_MODULE } from "../modules/booking"
import { OFICINA_MODULE } from "../modules/oficina"

/**
 * Workflow: Create Booking
 * 
 * Fluxo completo de criação de agendamento com validações e notificações
 * 1. Validar dados do booking
 * 2. Verificar disponibilidade da oficina
 * 3. Criar booking no banco
 * 4. Emitir evento para notificações
 * 5. Rollback automático em caso de falha
 */

interface CreateBookingInput {
  customer_id: string
  oficina_id: string
  vehicle_id: string
  service_ids: string[]
  scheduled_at: Date
  notes?: string
}

export const createBookingWorkflow = createWorkflow(
  "create-booking",
  async function (input: CreateBookingInput) {
    // Step 1: Validar oficina existe e está aprovada
    const workshop = this.step("validate-workshop", async () => {
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      const oficina = await oficinaService.retrieveOficina(input.oficina_id)
      
      if (!oficina) {
        throw new Error("Oficina não encontrada")
      }
      
      if (oficina.status !== "aprovado") {
        throw new Error("Esta oficina não está aprovada para receber agendamentos")
      }
      
      return oficina
    })

    // Step 2: Validar cliente existe
    const customer = this.step("validate-customer", async () => {
      const customerService = this.container.resolve(Modules.CUSTOMER)
      const cust = await customerService.retrieveCustomer(input.customer_id)
      
      if (!cust) {
        throw new Error("Cliente não encontrado")
      }
      
      return cust
    })

    // Step 3: Criar booking
    const booking = this.step("create-booking", async () => {
      const bookingService = this.container.resolve(BOOKING_MODULE)
      
      const newBooking = await bookingService.createBookings({
        customer_id: input.customer_id,
        oficina_id: input.oficina_id,
        vehicle_id: input.vehicle_id,
        scheduled_at: input.scheduled_at,
        status: "pendente_oficina",
        metadata: {
          service_ids: input.service_ids,
          notes: input.notes
        }
      })
      
      return newBooking
    })

    // Step 4: Emitir evento
    await this.step("emit-event", async () => {
      const eventBus = this.container.resolve("eventBus")
      
      await eventBus.emit("booking.created", {
        booking_id: booking.id,
        customer_id: input.customer_id,
        oficina_id: input.oficina_id,
        workshop_name: workshop.name,
        customer_name: customer.first_name
      })
    })

    return new WorkflowResponse(booking)
  }
)

/**
 * Workflow: Confirm Booking
 * 
 * Fluxo de confirmação de agendamento pela oficina
 */

interface ConfirmBookingInput {
  booking_id: string
  oficina_id: string
}

export const confirmBookingWorkflow = createWorkflow(
  "confirm-booking",
  async function (input: ConfirmBookingInput) {
    // Step 1: Buscar booking
    const booking = this.step("fetch-booking", async () => {
      const bookingService = this.container.resolve(BOOKING_MODULE)
      const bk = await bookingService.retrieveBooking(input.booking_id)
      
      if (!bk) {
        throw new Error("Agendamento não encontrado")
      }
      
      if (bk.oficina_id !== input.oficina_id) {
        throw new Error("Você não tem permissão para confirmar este agendamento")
      }
      
      if (bk.status !== "pendente_oficina") {
        throw new Error("Este agendamento não pode ser confirmado")
      }
      
      return bk
    })

    // Step 2: Atualizar status
    const updated = this.step("update-status", async () => {
      const bookingService = this.container.resolve(BOOKING_MODULE)
      
      return await bookingService.updateBookings(input.booking_id, {
        status: "confirmado",
        confirmed_at: new Date()
      })
    })

    // Step 3: Buscar dados para email
    const emailData = this.step("fetch-email-data", async () => {
      const customerService = this.container.resolve(Modules.CUSTOMER)
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      
      const customer = await customerService.retrieveCustomer(booking.customer_id)
      const oficina = await oficinaService.retrieveOficina(booking.oficina_id)
      
      return { customer, oficina }
    })

    // Step 4: Emitir evento para enviar email
    await this.step("emit-confirmed-event", async () => {
      const eventBus = this.container.resolve("eventBus")
      
      await eventBus.emit("booking.confirmed", {
        booking_id: input.booking_id,
        customer_id: booking.customer_id,
        oficina_id: input.oficina_id,
        workshop_name: emailData.oficina.name,
        service_title: booking.metadata?.service_title || "Serviço",
        scheduled_at: booking.scheduled_at
      })
    })

    return new WorkflowResponse(updated)
  }
)

/**
 * Workflow: Reject Booking
 * 
 * Fluxo de rejeição de agendamento pela oficina
 */

interface RejectBookingInput {
  booking_id: string
  oficina_id: string
  reason?: string
}

export const rejectBookingWorkflow = createWorkflow(
  "reject-booking",
  async function (input: RejectBookingInput) {
    // Step 1: Buscar booking
    const booking = this.step("fetch-booking", async () => {
      const bookingService = this.container.resolve(BOOKING_MODULE)
      const bk = await bookingService.retrieveBooking(input.booking_id)
      
      if (!bk) {
        throw new Error("Agendamento não encontrado")
      }
      
      if (bk.oficina_id !== input.oficina_id) {
        throw new Error("Você não tem permissão para rejeitar este agendamento")
      }
      
      if (bk.status !== "pendente_oficina") {
        throw new Error("Este agendamento não pode ser rejeitado")
      }
      
      return bk
    })

    // Step 2: Atualizar status
    const updated = this.step("update-status", async () => {
      const bookingService = this.container.resolve(BOOKING_MODULE)
      
      return await bookingService.updateBookings(input.booking_id, {
        status: "recusado",
        metadata: {
          ...booking.metadata,
          rejection_reason: input.reason || "Não informado",
          rejected_at: new Date().toISOString()
        }
      })
    })

    // Step 3: Buscar dados para email
    const emailData = this.step("fetch-email-data", async () => {
      const customerService = this.container.resolve(Modules.CUSTOMER)
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      
      const customer = await customerService.retrieveCustomer(booking.customer_id)
      const oficina = await oficinaService.retrieveOficina(booking.oficina_id)
      
      return { customer, oficina }
    })

    // Step 4: Emitir evento para enviar email
    await this.step("emit-rejected-event", async () => {
      const eventBus = this.container.resolve("eventBus")
      
      await eventBus.emit("booking.rejected", {
        booking_id: input.booking_id,
        customer_id: booking.customer_id,
        oficina_id: input.oficina_id,
        workshop_name: emailData.oficina.name,
        service_title: booking.metadata?.service_title || "Serviço",
        reason: input.reason
      })
    })

    return new WorkflowResponse(updated)
  }
)

