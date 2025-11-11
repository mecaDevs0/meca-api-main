import { MedusaContainer } from "@medusajs/medusa"
import { EntityManager } from "typeorm"
import { Booking } from "../models/booking"
import { Vehicle } from "../models/vehicle"
import { Workshop } from "../models/workshop"
import { Service } from "../models/service"

export default async function onBookingPaidHandler(
  container: MedusaContainer,
  data: { bookingId: string }
) {
  const manager = container.resolve<EntityManager>("manager")
  
  try {
    console.log(`🔄 Processando histórico de manutenção para booking: ${data.bookingId}`)
    
    // Buscar dados completos do agendamento
    const booking = await manager.findOne(Booking, {
      where: { id: data.bookingId },
      relations: [
        'customer',
        'vehicle', 
        'workshop',
        'service'
      ]
    })

    if (!booking) {
      console.error(`❌ Booking não encontrado: ${data.bookingId}`)
      return
    }

    if (booking.status !== 'paid') {
      console.log(`⚠️ Booking ${data.bookingId} não está pago, status: ${booking.status}`)
      return
    }

    // Criar registro no histórico de manutenção
    const maintenanceRecord = {
      id: `maint_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
      vehicleId: booking.vehicle.id,
      customerId: booking.customer.id,
      workshopId: booking.workshop.id,
      workshopName: booking.workshop.name,
      serviceId: booking.service.id,
      serviceName: booking.service.name,
      serviceDescription: booking.service.description,
      completionDate: booking.completed_at || new Date(),
      pricePaid: booking.total_amount,
      notes: booking.notes,
      workshopNotes: booking.workshop_notes,
      createdAt: new Date(),
      updatedAt: new Date()
    }

    // Salvar no banco de dados
    await manager.query(`
      INSERT INTO maintenance_history (
        id, vehicle_id, customer_id, workshop_id, workshop_name,
        service_id, service_name, service_description, completion_date,
        price_paid, notes, workshop_notes, created_at, updated_at
      ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13, $14)
    `, [
      maintenanceRecord.id,
      maintenanceRecord.vehicleId,
      maintenanceRecord.customerId,
      maintenanceRecord.workshopId,
      maintenanceRecord.workshopName,
      maintenanceRecord.serviceId,
      maintenanceRecord.serviceName,
      maintenanceRecord.serviceDescription,
      maintenanceRecord.completionDate,
      maintenanceRecord.pricePaid,
      maintenanceRecord.notes,
      maintenanceRecord.workshopNotes,
      maintenanceRecord.createdAt,
      maintenanceRecord.updatedAt
    ])

    console.log(`✅ Histórico de manutenção criado: ${maintenanceRecord.id}`)
    
    // Disparar notificação para o cliente
    await container.resolve("notificationService").send({
      type: "maintenance_history_created",
      customerId: booking.customer.id,
      data: {
        vehiclePlate: booking.vehicle.plate,
        serviceName: booking.service.name,
        workshopName: booking.workshop.name,
        completionDate: maintenanceRecord.completionDate
      }
    })

  } catch (error) {
    console.error(`❌ Erro ao processar histórico de manutenção:`, error)
    throw error
  }
}












