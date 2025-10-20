import { defineRouteConfig } from "@medusajs/admin-sdk"
import { Container, Heading, Badge, Table, Button } from "@medusajs/ui"
import { useState, useEffect } from "react"
import { useNavigate } from "react-router-dom"

/**
 * Página Customizada: Aprovação de Oficinas
 * 
 * Permite ao admin visualizar oficinas pendentes e aprovar/rejeitar
 */

const WorkshopsApprovalPage = () => {
  const [oficinas, setOficinas] = useState([])
  const [loading, setLoading] = useState(true)
  const navigate = useNavigate()
  
  useEffect(() => {
    fetchPendingWorkshops()
  }, [])
  
  const fetchPendingWorkshops = async () => {
    try {
      setLoading(true)
      
      // Chamar API customizada
      const response = await fetch("/admin/workshops?status=pendente", {
        credentials: "include"
      })
      
      const data = await response.json()
      setOficinas(data.oficinas || [])
      
    } catch (error) {
      console.error("Erro ao buscar oficinas:", error)
    } finally {
      setLoading(false)
    }
  }
  
  const handleApprove = async (oficinaId: string) => {
    try {
      const response = await fetch(`/admin/workshops/${oficinaId}/approve`, {
        method: "POST",
        credentials: "include"
      })
      
      if (response.ok) {
        alert("Oficina aprovada com sucesso!")
        fetchPendingWorkshops()
      } else {
        alert("Erro ao aprovar oficina")
      }
      
    } catch (error) {
      console.error("Erro:", error)
      alert("Erro ao aprovar oficina")
    }
  }
  
  const handleReject = async (oficinaId: string) => {
    const reason = prompt("Motivo da rejeição:")
    
    if (!reason) return
    
    try {
      const response = await fetch(`/admin/workshops/${oficinaId}/reject`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ reason }),
        credentials: "include"
      })
      
      if (response.ok) {
        alert("Oficina rejeitada")
        fetchPendingWorkshops()
      } else {
        alert("Erro ao rejeitar oficina")
      }
      
    } catch (error) {
      console.error("Erro:", error)
      alert("Erro ao rejeitar oficina")
    }
  }
  
  if (loading) {
    return <Container><Heading level="h1">Carregando...</Heading></Container>
  }
  
  return (
    <Container>
      <div className="flex items-center justify-between mb-4">
        <Heading level="h1">Aprovação de Oficinas</Heading>
        <Badge color="orange">{oficinas.length} Pendentes</Badge>
      </div>
      
      {oficinas.length === 0 ? (
        <div className="text-center py-8 text-gray-500">
          Nenhuma oficina pendente de aprovação
        </div>
      ) : (
        <Table>
          <Table.Header>
            <Table.Row>
              <Table.HeaderCell>Nome</Table.HeaderCell>
              <Table.HeaderCell>CNPJ</Table.HeaderCell>
              <Table.HeaderCell>Email</Table.HeaderCell>
              <Table.HeaderCell>Telefone</Table.HeaderCell>
              <Table.HeaderCell>Cidade</Table.HeaderCell>
              <Table.HeaderCell>Status</Table.HeaderCell>
              <Table.HeaderCell>Ações</Table.HeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {oficinas.map((oficina: any) => (
              <Table.Row key={oficina.id}>
                <Table.Cell>{oficina.name}</Table.Cell>
                <Table.Cell>{oficina.cnpj}</Table.Cell>
                <Table.Cell>{oficina.email}</Table.Cell>
                <Table.Cell>{oficina.phone}</Table.Cell>
                <Table.Cell>{oficina.address?.cidade || "-"}</Table.Cell>
                <Table.Cell>
                  <Badge color="orange">{oficina.status}</Badge>
                </Table.Cell>
                <Table.Cell>
                  <div className="flex gap-2">
                    <Button
                      size="small"
                      variant="primary"
                      onClick={() => handleApprove(oficina.id)}
                    >
                      Aprovar
                    </Button>
                    <Button
                      size="small"
                      variant="danger"
                      onClick={() => handleReject(oficina.id)}
                    >
                      Rejeitar
                    </Button>
                  </div>
                </Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table>
      )}
    </Container>
  )
}

export const config = defineRouteConfig({
  label: "Aprovar Oficinas",
  icon: "BuildingStorefront",
})

export default WorkshopsApprovalPage

