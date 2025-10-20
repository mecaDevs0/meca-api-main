import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

export const AUTHENTICATE = false

/**
 * GET /public/master-services
 * 
 * Lista todos os serviços padrão disponíveis no sistema (sem autenticação)
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  
  try {
    // Buscar todos os produtos que são serviços padrão
    // Assumindo que serviços padrão têm metadata.is_master_service = true
    const masterServices = await productModuleService.listProducts({
      metadata: {
        is_master_service: true
      }
    })
    
    // Se não houver serviços padrão, criar alguns exemplos
    if (masterServices.length === 0) {
      const defaultServices = [
        {
          id: 'master_service_1',
          title: 'Troca de Óleo',
          description: 'Troca completa de óleo do motor com filtro de óleo incluído',
          category: 'Manutenção',
          estimated_duration: 30,
          base_price: 8000, // em centavos
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQxKSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDEiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkY2QjQwIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0ZGNkE0MCIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iI0ZGNkI0MCIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSI4IiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNDhIMTYiIHN0cm9rZT0iI0ZGNkI0MCIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTEwNCA0OEg2OCIgc3Ryb2tlPSIjRkY2QjQwIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_2',
          title: 'Revisão Preventiva',
          description: 'Revisão completa do veículo incluindo filtros, fluidos e componentes',
          category: 'Manutenção',
          estimated_duration: 120,
          base_price: 15000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQyKSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDIiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjMTA5Nzg1Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzA4NjY0MyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzEwOTc4NSIvLz4KPHBhdGggZD0iTTU2IDYwSDQ0VjY4SDU2VjYwWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDU2SDQ4VjcySDUyVjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDU2SDY0VjcySDY4VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiMxMDk3ODUiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iIzEwOTc4NSIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_3',
          title: 'Alinhamento e Balanceamento',
          description: 'Alinhamento da direção e balanceamento das rodas para melhor dirigibilidade',
          category: 'Suspensão',
          estimated_duration: 60,
          base_price: 12000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQzKSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDMiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjMDA5OUZGIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzAwODBFRiIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzAwOTlGRiIvLz4KPHBhdGggZD0iTTU2IDYwSDQ0VjY4SDU2VjYwWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDU2SDQ4VjcySDUyVjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDU2SDY0VjcySDY4VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiMwMDk5RkYiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iIzAwOTlGRiIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_4',
          title: 'Troca de Pneus',
          description: 'Troca completa de pneus com balanceamento incluído',
          category: 'Pneus',
          estimated_duration: 45,
          base_price: 20000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ0KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDQiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkY0NDQ0Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0VGMzMzMyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iI0ZGNDQ0NCIvLz4KPHBhdGggZD0iTTU2IDYwSDQ0VjY4SDU2VjYwWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDU2SDQ4VjcySDUyVjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDU2SDY0VjcySDY4VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiNGRjQ0NDQiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iI0ZGNDQ0NCIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_5',
          title: 'Freios',
          description: 'Revisão e troca de pastilhas e discos de freio',
          category: 'Freios',
          estimated_duration: 90,
          base_price: 18000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ1KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDUiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjOTk5OTk5Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzc3Nzc3NyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzk5OTk5OSIvLz4KPHBhdGggZD0iTTU2IDYwSDQ0VjY4SDU2VjYwWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDU2SDQ4VjcySDUyVjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDU2SDY0VjcySDY4VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiM5OTk5OTkiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iIzk5OTk5OSIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_6',
          title: 'Ar Condicionado',
          description: 'Manutenção e recarga do sistema de ar condicionado',
          category: 'Climatização',
          estimated_duration: 75,
          base_price: 14000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ2KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDYiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjMDBDQjk3NyIvPgo8c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiMwMEI4NkQiLz4KPC9saW5lYXJHcmFkaWVudD4KPC9kZWZzPgo8cGF0aCBkPSJNNjAgMjBDNDAuMiAyMCAyNCAzNi4yIDI0IDU2VjEwNEMyNCAxMjMuOCA0MC4yIDE0MCA2MCAxNDBDNzkuOCAxNDAgOTYgMTIzLjggOTYgMTA0VjU2Qzk2IDM2LjIgNzkuOCAyMCA2MCAyMFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMSIvPgo8cGF0aCBkPSJNNjAgMjhDNDQuNTMgMjggMzIgNDAuNTMgMzIgNTZWMTAwQzMyIDExNS40NyA0NC41MyAxMjggNjAgMTI4Qzc1LjQ3IDEyOCA4OCAxMTUuNDcgODggMTAwVjU2Qzg4IDQwLjUzIDc1LjQ3IDI4IDYwIDI4WiIgZmlsbD0id2hpdGUiIGZpbGwtb3BhY2l0eT0iMC4yIi8+CjxjaXJjbGUgY3g9IjYwIiBjeT0iNjQiIHI9IjE2IiBmaWxsPSIjMDBDQjk3NyIvPgo8cGF0aCBkPSJNNTYgNjBINDRWNjhINTZWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNTZINDhWNzJINTJWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjggNTZINjRWNzJINjhaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNDAgNDhIMjQiIHN0cm9rZT0iIzAwQ0I5NzciIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iIzAwQ0I5NzciIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+Cjwvc3ZnPgo=',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_7',
          title: 'Bateria',
          description: 'Troca de bateria e teste do sistema elétrico',
          category: 'Elétrica',
          estimated_duration: 30,
          base_price: 10000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ3KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDciIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkZENzAwIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0ZGRDcwMCIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iI0ZGRDcwMCIvPgo8cGF0aCBkPSJNNTYgNjBINDRWNjhINTZWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNTZINDhWNzJINTJWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjggNTZINjRWNzJINjhaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNDAgNDhIMjQiIHN0cm9rZT0iI0ZGRDcwMCIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTEwMCA0OEg2OCIgc3Ryb2tlPSIjRkZENzAwIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_8',
          title: 'Embreagem',
          description: 'Troca de embreagem e componentes relacionados',
          category: 'Transmissão',
          estimated_duration: 180,
          base_price: 25000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ4KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDgiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjOUM0M0ZGIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzg0M0ZGRiIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzlDNDNGRiIvPgo8cGF0aCBkPSJNNTYgNjBINDRWNjhINTZWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNTZINDhWNzJINTJWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjggNTZINjRWNzJINjhaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNDAgNDhIMjQiIHN0cm9rZT0iIzlDNDNGRiIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTEwMCA0OEg2OCIgc3Ryb2tlPSIjOUM0M0ZGIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_9',
          title: 'Suspensão',
          description: 'Revisão e troca de componentes da suspensão',
          category: 'Suspensão',
          estimated_duration: 120,
          base_price: 16000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ5KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDkiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkY5MDAwIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0VGNzAwMCIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iI0ZGOTAwMCIvPgo8cGF0aCBkPSJNNTYgNjBINDRWNjhINTZWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNTZINDhWNzJINTJWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjggNTZINjRWNzJINjhaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNDAgNDhIMjQiIHN0cm9rZT0iI0ZGOTAwMCIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTEwMCA0OEg2OCIgc3Ryb2tlPSIjRkY5MDAwIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_10',
          title: 'Diagnóstico Eletrônico',
          description: 'Diagnóstico completo do sistema eletrônico do veículo',
          category: 'Diagnóstico',
          estimated_duration: 60,
          base_price: 8000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQxMCkiLz4KPG1lZnM+CjxsaW5lYXJHcmFkaWVudCBpZD0iZ3JhZGllbnQxMCIgeDE9IjAiIHkxPSIwIiB4Mj0iMTIwIiB5Mj0iMTIwIiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSI+CjxzdG9wIHN0b3AtY29sb3I9IiM2NjY2NjYiLz4KPHN0b3Agb2Zmc2V0PSIxIiBzdG9wLWNvbG9yPSIjNDQ0NDQ0Ii8+CjwvbGluZWFyR3JhZGllbnQ+CjwvZGVmcz4KPHBhdGggZD0iTTYwIDIwQzQwLjIgMjAgMjQgMzYuMiAyNCA1NlYxMDRDMjQgMTIzLjggNDAuMiAxNDAgNjAgMTQwQzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzY2NjY2NiIvPgo8cGF0aCBkPSJNNTYgNjBINDRWNjhINTZWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNTZINDhWNzJINTJWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjggNTZINjRWNzJINjhaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNDAgNDhIMjQiIHN0cm9rZT0iIzY2NjY2NiIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTEwMCA0OEg2OCIgc3Ryb2tlPSIjNjY2NjY2IiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K',
          is_active: true,
          created_at: new Date().toISOString()
        }
      ]
      
      return res.json({
        services: defaultServices,
        count: defaultServices.length,
      })
    }
    
    return res.json({
      services: masterServices,
      count: masterServices.length,
    })
    
  } catch (error) {
    console.error("Erro ao listar serviços padrão:", error)
    
    return res.status(500).json({
      message: "Erro ao listar serviços padrão",
      error: error.message
    })
  }
}




