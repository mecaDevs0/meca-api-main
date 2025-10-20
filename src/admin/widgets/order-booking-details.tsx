import { defineWidgetConfig } from "@medusajs/admin-sdk"
import { Container, Heading, Text } from "@medusajs/ui"
import { useState, useEffect } from "react"
import { AdminOrder } from "@medusajs/framework/types"

/**
 * Widget: Detalhes do Agendamento no Pedido
 * 
 * Exibe informações do booking associado a um pedido
 * Injetado na zona: order.details.after
 */

const OrderBookingDetailsWidget = ({ data }: { data: AdminOrder }) => {
  const [booking, setBooking] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  
  useEffect(() => {
    fetchBookingDetails()
  }, [data.id])
  
  const fetchBookingDetails = async () => {
    try {
      setLoading(true)
      
      // Verificar se este pedido tem um booking associado nos metadata
      const bookingId = data.metadata?.booking_id
      
      if (!bookingId) {
        setLoading(false)
        return
      }
      
      // Buscar detalhes do booking
      // Nota: Esta seria uma API interna/admin para buscar bookings
      // Por enquanto, mostrar apenas o que está nos metadata
      setBooking({
        id: bookingId,
        ...data.metadata
      })
      
    } catch (error) {
      console.error("Erro ao buscar detalhes do agendamento:", error)
    } finally {
      setLoading(false)
    }
  }
  
  if (loading) {
    return <Container><Text>Carregando...</Text></Container>
  }
  
  if (!booking) {
    return null // Não é um pedido de agendamento
  }
  
  return (
    <Container className="mt-4">
      <Heading level="h2" className="mb-4">
        📅 Detalhes do Agendamento
      </Heading>
      
      <div className="grid grid-cols-2 gap-4">
        <div>
          <Text className="text-sm text-gray-500">ID do Agendamento</Text>
          <Text className="font-medium">{booking.id}</Text>
        </div>
        
        <div>
          <Text className="text-sm text-gray-500">Data do Agendamento</Text>
          <Text className="font-medium">
            {booking.appointment_date 
              ? new Date(booking.appointment_date).toLocaleString('pt-BR')
              : "-"}
          </Text>
        </div>
        
        <div>
          <Text className="text-sm text-gray-500">Status do Agendamento</Text>
          <Text className="font-medium">{booking.status || "-"}</Text>
        </div>
        
        <div>
          <Text className="text-sm text-gray-500">Veículo</Text>
          <Text className="font-medium">
            {booking.vehicle_snapshot 
              ? `${booking.vehicle_snapshot.marca} ${booking.vehicle_snapshot.modelo} (${booking.vehicle_snapshot.ano})`
              : "-"}
          </Text>
        </div>
        
        {booking.vehicle_snapshot?.placa && (
          <div>
            <Text className="text-sm text-gray-500">Placa</Text>
            <Text className="font-medium">{booking.vehicle_snapshot.placa}</Text>
          </div>
        )}
        
        {booking.customer_notes && (
          <div className="col-span-2">
            <Text className="text-sm text-gray-500">Observações do Cliente</Text>
            <Text className="font-medium">{booking.customer_notes}</Text>
          </div>
        )}
        
        {booking.oficina_notes && (
          <div className="col-span-2">
            <Text className="text-sm text-gray-500">Observações da Oficina</Text>
            <Text className="font-medium">{booking.oficina_notes}</Text>
          </div>
        )}
      </div>
      
      {data.metadata?.meca_commission_amount && (
        <div className="mt-4 p-4 bg-blue-50 rounded">
          <Text className="text-sm font-semibold text-blue-700">
            💰 Comissão MECA: R$ {(data.metadata.meca_commission_amount / 100).toFixed(2)} 
            ({(data.metadata.meca_commission_rate * 100)}%)
          </Text>
        </div>
      )}
    </Container>
  )
}

export const config = defineWidgetConfig({
  zone: "order.details.after",
})

export default OrderBookingDetailsWidget

