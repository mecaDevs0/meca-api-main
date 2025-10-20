import { medusaIntegrationTestRunner } from "@medusajs/test-utils"

jest.setTimeout(50000)

medusaIntegrationTestRunner({
  testSuite: ({ getContainer, api }) => {
    describe("MECA v2.0 - Vehicle API Tests", () => {
      let customerId: string
      let vehicleId: string
      
      beforeAll(async () => {
        // Criar um customer para testes
        const customerResponse = await api.post("/admin/customers", {
          email: "cliente.teste@meca.com.br",
          first_name: "Cliente",
          last_name: "Teste",
        })
        
        customerId = customerResponse.data.customer.id
      })
      
      describe("POST /store/vehicles", () => {
        it("deve criar um novo veículo para o cliente", async () => {
          const response = await api.post("/store/vehicles", {
            marca: "Toyota",
            modelo: "Corolla",
            ano: 2020,
            placa: "ABC-1234",
            cor: "Prata",
            combustivel: "flex",
          }, {
            headers: {
              // Simular autenticação do cliente
              "x-customer-id": customerId
            }
          })
          
          expect(response.status).toBe(201)
          expect(response.data.vehicle).toBeDefined()
          expect(response.data.vehicle.marca).toBe("Toyota")
          expect(response.data.vehicle.customer_id).toBe(customerId)
          
          vehicleId = response.data.vehicle.id
        })
        
        it("deve retornar erro se campos obrigatórios estiverem faltando", async () => {
          const response = await api.post("/store/vehicles", {
            marca: "Ford",
            // Faltando modelo, ano e placa
          }, {
            headers: {
              "x-customer-id": customerId
            }
          })
          
          expect(response.status).toBe(400)
          expect(response.data.message).toContain("obrigatórios")
        })
      })
      
      describe("GET /store/vehicles", () => {
        it("deve listar os veículos do cliente autenticado", async () => {
          const response = await api.get("/store/vehicles", {
            headers: {
              "x-customer-id": customerId
            }
          })
          
          expect(response.status).toBe(200)
          expect(response.data.vehicles).toBeDefined()
          expect(Array.isArray(response.data.vehicles)).toBe(true)
          expect(response.data.vehicles.length).toBeGreaterThan(0)
        })
      })
      
      describe("PUT /store/vehicles/:id", () => {
        it("deve atualizar um veículo do cliente", async () => {
          const response = await api.put(`/store/vehicles/${vehicleId}`, {
            cor: "Preto",
            km_atual: 50000,
          }, {
            headers: {
              "x-customer-id": customerId
            }
          })
          
          expect(response.status).toBe(200)
          expect(response.data.vehicle.cor).toBe("Preto")
          expect(response.data.vehicle.km_atual).toBe(50000)
        })
      })
      
      describe("DELETE /store/vehicles/:id", () => {
        it("deve deletar um veículo do cliente", async () => {
          const response = await api.delete(`/store/vehicles/${vehicleId}`, {
            headers: {
              "x-customer-id": customerId
            }
          })
          
          expect(response.status).toBe(204)
        })
      })
    })
  },
})

