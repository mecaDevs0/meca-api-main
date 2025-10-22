import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { getPagBankService } from "../../../../../../services/pagbank"

/**
 * GET /store/workshops/me/pagbank/test
 * 
 * Testar conexão com PagBank
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  try {
    const pagBankService = getPagBankService()
    
    // Testar a conexão fazendo uma requisição simples
    // Como não temos um endpoint de "health check" no PagBank, vamos tentar listar contas
    const testResponse = await pagBankService.listPayments('test', 1, 0)
    
    return res.json({
      success: true,
      message: "Conexão com PagBank estabelecida com sucesso",
      data: {
        service_status: "connected",
        token_configured: true,
        base_url: pagBankService['config'].baseUrl,
        test_response: testResponse
      }
    })
    
  } catch (error: any) {
    console.error("Erro ao testar conexão PagBank:", error)
    
    return res.json({
      success: false,
      message: "Erro ao conectar com PagBank",
      error: error.message,
      data: {
        service_status: "error",
        token_configured: true,
        base_url: getPagBankService()['config'].baseUrl
      }
    })
  }
}
















