import { createWorkflow, WorkflowResponse } from "@medusajs/framework/workflows"
import { OFICINA_MODULE } from "../modules/oficina"
import { OficinaStatus } from "../modules/oficina/models/oficina"

/**
 * Workflow: Workshop Approval
 * 
 * Fluxo completo de aprovação de oficina
 * 1. Validar oficina está pendente
 * 2. Aprovar oficina
 * 3. Emitir evento para enviar email
 * 4. Ativar serviços da oficina
 */

interface ApproveWorkshopInput {
  workshop_id: string
}

export const approveWorkshopWorkflow = createWorkflow(
  "approve-workshop",
  async function (input: ApproveWorkshopInput) {
    // Step 1: Buscar e validar oficina
    const workshop = this.step("validate-workshop", async () => {
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      const oficina = await oficinaService.retrieveOficina(input.workshop_id)
      
      if (!oficina) {
        throw new Error("Oficina não encontrada")
      }
      
      if (oficina.status !== OficinaStatus.PENDENTE) {
        throw new Error(`Oficina já foi processada. Status atual: ${oficina.status}`)
      }
      
      return oficina
    })

    // Step 2: Aprovar oficina
    const updated = this.step("approve-workshop", async () => {
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      
      const updatedWorkshops = await oficinaService.updateOficinas([{
        id: input.workshop_id,
        status: OficinaStatus.APROVADO
      }])
      
      return Array.isArray(updatedWorkshops) ? updatedWorkshops[0] : updatedWorkshops
    })

    // Step 3: Emitir evento para enviar email
    await this.step("emit-approved-event", async () => {
      const eventBus = this.container.resolve("eventBus")
      
      await eventBus.emit("workshop.approved", {
        workshop_id: input.workshop_id,
        email: workshop.email,
        name: workshop.name
      })
    })

    // Step 4: Ativar configurações adicionais (se necessário)
    await this.step("activate-services", async () => {
      // Aqui você pode ativar serviços adicionais, configurar pagamentos, etc.
      const logger = this.container.resolve("logger")
      logger.info(`Workshop ${workshop.name} approved and activated`)
    })

    return new WorkflowResponse(updated)
  }
)

/**
 * Workflow: Workshop Rejection
 * 
 * Fluxo de rejeição de oficina
 */

interface RejectWorkshopInput {
  workshop_id: string
  reason: string
}

export const rejectWorkshopWorkflow = createWorkflow(
  "reject-workshop",
  async function (input: RejectWorkshopInput) {
    // Step 1: Buscar e validar oficina
    const workshop = this.step("validate-workshop", async () => {
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      const oficina = await oficinaService.retrieveOficina(input.workshop_id)
      
      if (!oficina) {
        throw new Error("Oficina não encontrada")
      }
      
      if (oficina.status !== OficinaStatus.PENDENTE) {
        throw new Error(`Oficina já foi processada. Status atual: ${oficina.status}`)
      }
      
      return oficina
    })

    // Step 2: Rejeitar oficina
    const updated = this.step("reject-workshop", async () => {
      const oficinaService = this.container.resolve(OFICINA_MODULE)
      
      const updatedWorkshops = await oficinaService.updateOficinas([{
        id: input.workshop_id,
        status: OficinaStatus.REJEITADO,
        metadata: {
          ...workshop.metadata,
          rejection_reason: input.reason,
          rejected_at: new Date().toISOString()
        }
      }])
      
      return Array.isArray(updatedWorkshops) ? updatedWorkshops[0] : updatedWorkshops
    })

    // Step 3: Emitir evento (se quiser enviar email de rejeição)
    await this.step("emit-rejected-event", async () => {
      const eventBus = this.container.resolve("eventBus")
      
      await eventBus.emit("workshop.rejected", {
        workshop_id: input.workshop_id,
        email: workshop.email,
        name: workshop.name,
        reason: input.reason
      })
    })

    return new WorkflowResponse(updated)
  }
)

