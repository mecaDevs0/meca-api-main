import { medusaIntegrationTestRunner } from "@medusajs/test-utils"

jest.setTimeout(50000)

medusaIntegrationTestRunner({
  testSuite: ({ getContainer, api }) => {
    describe("MECA v2.0 - Workshop API Tests", () => {
      let oficinaId: string
      
      describe("POST /store/workshops", () => {
        it("deve cadastrar uma nova oficina", async () => {
          const response = await api.post("/store/workshops", {
            email: "oficina.teste@meca.com.br",
            password: "senha123",
            name: "Oficina Teste Ltda",
            cnpj: "12.345.678/0001-90",
            phone: "(11) 98765-4321",
            address: {
              rua: "Rua Teste",
              numero: "123",
              bairro: "Centro",
              cidade: "São Paulo",
              estado: "SP",
              cep: "01234-567",
            },
            description: "Oficina especializada em manutenção preventiva",
          })
          
          expect(response.status).toBe(201)
          expect(response.data.oficina).toBeDefined()
          expect(response.data.oficina.name).toBe("Oficina Teste Ltda")
          expect(response.data.oficina.status).toBe("pendente")
          expect(response.data.user).toBeDefined()
          
          oficinaId = response.data.oficina.id
        })
      })
      
      describe("GET /admin/workshops", () => {
        it("deve listar oficinas pendentes", async () => {
          const response = await api.get("/admin/workshops?status=pendente")
          
          expect(response.status).toBe(200)
          expect(response.data.oficinas).toBeDefined()
          expect(Array.isArray(response.data.oficinas)).toBe(true)
        })
      })
      
      describe("POST /admin/workshops/:id/approve", () => {
        it("deve aprovar uma oficina pendente", async () => {
          const response = await api.post(`/admin/workshops/${oficinaId}/approve`)
          
          expect(response.status).toBe(200)
          expect(response.data.oficina.status).toBe("aprovado")
          expect(response.data.message).toContain("aprovada")
        })
        
        it("não deve aprovar uma oficina já aprovada", async () => {
          const response = await api.post(`/admin/workshops/${oficinaId}/approve`)
          
          expect(response.status).toBe(400)
          expect(response.data.message).toContain("não pode ser aprovada")
        })
      })
      
      describe("GET /store/workshops", () => {
        it("deve listar apenas oficinas aprovadas", async () => {
          const response = await api.get("/store/workshops")
          
          expect(response.status).toBe(200)
          expect(response.data.oficinas).toBeDefined()
          
          // Verificar que todas estão aprovadas
          response.data.oficinas.forEach((oficina: any) => {
            expect(oficina.status).toBe("aprovado")
          })
        })
      })
    })
  },
})

