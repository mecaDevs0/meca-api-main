import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { Modules } from "@medusajs/framework/utils"

/**
 * GET /admin/master-services
 * 
 * Lista todos os serviços padrão disponíveis no sistema
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
          title: 'Troca de Pneus',
          description: 'Troca de pneus com balanceamento e alinhamento',
          category: 'Pneus',
          estimated_duration: 60,
          base_price: 12000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQzKSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDMiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjMzc0MTUxIi8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzI5MzI0MyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIyMCIgZmlsbD0iIzM3NDE1MSIvLz4KPGNpcmNsZSBjeD0iNjAiIGN5PSI2NCIgcj0iMTIiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMyIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSI4IiBmaWxsPSIjMzc0MTUxIi8+CjxwYXRoIGQ9Ik00MCA0OEgyNCIgc3Ryb2tlPSIjMzc0MTUxIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8cGF0aCBkPSJNMTAgNDhIMjQiIHN0cm9rZT0iIzM3NDE1MSIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTExMCA0OEg5NiIgc3Ryb2tlPSIjMzc0MTUxIiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8cGF0aCBkPSJNMTEwIDQ4SDk2IiBzdHJva2U9IiMzNzQxNTEiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+Cjwvc3ZnPgo=',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_4',
          title: 'Reparo de Freios',
          description: 'Troca de pastilhas e discos de freio',
          category: 'Freios',
          estimated_duration: 90,
          base_price: 18000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ0KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDQiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkY0NDQ0Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0VGMzMzMyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iI0ZGNDQ0NCIvLz4KPGNpcmNsZSBjeD0iNjAiIGN5PSI2NCIgcj0iOCIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTU2IDU2SDQ0VjcySDU2VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDUySDQ4Vjc2SDUyVjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDUySDY0Vjc2SDY4VjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiNGRjQ0NDQiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iI0ZGNDQ0NCIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_5',
          title: 'Diagnóstico Eletrônico',
          description: 'Diagnóstico completo do sistema eletrônico do veículo',
          category: 'Eletrônica',
          estimated_duration: 45,
          base_price: 10000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ1KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDUiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjMzc4M0Y2Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzI1NjNlYiIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8cmVjdCB4PSI0OCIgeT0iNTYiIHdpZHRoPSIyNCIgaGVpZ2h0PSIxNiIgcng9IjQiIGZpbGw9IiMzNzgzRjYiLz4KPHJlY3QgeD0iNTIiIHk9IjYwIiB3aWR0aD0iMTYiIGhlaWdodD0iOCIgcng9IjIiIGZpbGw9IndoaXRlIi8+CjxwYXRoIGQ9Ik00MCA0OEgyNCIgc3Ryb2tlPSIjMzc4M0Y2IiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8cGF0aCBkPSJNMTAgNDhIMjQiIHN0cm9rZT0iIzM3ODNGNiIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTExMCA0OEg5NiIgc3Ryb2tlPSIjMzc4M0Y2IiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8cGF0aCBkPSJNMTEwIDQ4SDk2IiBzdHJva2U9IiMzNzgzRjYiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+Cjwvc3ZnPgo=',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_6',
          title: 'Troca de Bateria',
          description: 'Troca de bateria com teste do sistema elétrico',
          category: 'Eletrônica',
          estimated_duration: 30,
          base_price: 15000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ2KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDYiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkU5NTQ1Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0ZENzM0NyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8cmVjdCB4PSI0OCIgeT0iNTIiIHdpZHRoPSIyNCIgaGVpZ2h0PSIyNCIgcng9IjQiIGZpbGw9IiNGRTk1NDUiLz4KPHJlY3QgeD0iNTAiIHk9IjU0IiB3aWR0aD0iMjAiIGhlaWdodD0iMjAiIHJ4PSIyIiBmaWxsPSJ3aGl0ZSIvPgo8Y2lyY2xlIGN4PSI1NiIgY3k9IjY0IiByPSI0IiBmaWxsPSIjRkU5NTQ1Ii8+CjxjaXJjbGUgY3g9IjY0IiBjeT0iNjQiIHI9IjQiIGZpbGw9IiNGRTk1NDUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiNGRTk1NDUiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iI0ZFOTU0NSIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_7',
          title: 'Ar Condicionado',
          description: 'Manutenção e recarga do sistema de ar condicionado',
          category: 'Climatização',
          estimated_duration: 60,
          base_price: 12000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ3KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDciIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjMDZCNkE3Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzA4OTE5MSIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzA2QjZBNyIvLz4KPGNpcmNsZSBjeD0iNjAiIGN5PSI2NCIgcj0iOCIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTU2IDU2SDQ0VjcySDU2VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDUySDQ4Vjc2SDUyVjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDUySDY0Vjc2SDY4VjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiMwNkI2QTciIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iIzA2QjZBNyIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_8',
          title: 'Suspensão',
          description: 'Reparo e manutenção do sistema de suspensão',
          category: 'Suspensão',
          estimated_duration: 120,
          base_price: 25000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ4KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDgiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjOUI1Q0Y2Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iIzc0MzNkNiIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iIzlCNUNGNiIvLz4KPGNpcmNsZSBjeD0iNjAiIGN5PSI2NCIgcj0iOCIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTU2IDU2SDQ0VjcySDU2VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDUySDQ4Vjc2SDUyVjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDUySDY0Vjc2SDY4VjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiM5QjVDRjYiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iIzlCNUNGNiIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_9',
          title: 'Embreagem',
          description: 'Troca de kit de embreagem completo',
          category: 'Transmissão',
          estimated_duration: 180,
          base_price: 35000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQ5KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDkiIHgxPSIwIiB5MT0iMCIgeDI9IjEyMCIgeTI9IjEyMCIgZ3JhZGllbnRVbml0cz0idXNlclNwYWNlT25Vc2UiPgo8c3RvcCBzdG9wLWNvbG9yPSIjRkU5NTQ1Ii8+CjxzdG9wIG9mZnNldD0iMSIgc3RvcC1jb2xvcj0iI0ZENzM0NyIvPgo8L2xpbmVhckdyYWRpZW50Pgo8L2RlZnM+CjxwYXRoIGQ9Ik02MCAyMEM0MC4yIDIwIDI0IDM2LjIgMjQgNTZWMTA0QzI0IDEyMy44IDQwLjIgMTQwIDYwIDE0MEMzc5LjggMTQwIDk2IDEyMy44IDk2IDEwNFY1NkM5NiAzNi4yIDc5LjggMjAgNjAgMjBaIiBmaWxsPSJ3aGl0ZSIgZmlsbC1vcGFjaXR5PSIwLjEiLz4KPHBhdGggZD0iTTYwIDI4QzQ0LjUzIDI4IDMyIDQwLjUzIDMyIDU2VjEwMEMzMiAxMTUuNDcgNDQuNTMgMTI4IDYwIDEyOEM3NS40NyAxMjggODggMTE1LjQ3IDg4IDEwMFY1NkM4OCA0MC41MyA3NS40NyAyOCA2MCAyOFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMiIvPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSIxNiIgZmlsbD0iI0ZFOTU0NSIvLz4KPGNpcmNsZSBjeD0iNjAiIGN5PSI2NCIgcj0iOCIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTU2IDU2SDQ0VjcySDU2VjU2WiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTUyIDUySDQ4Vjc2SDUyVjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTY4IDUySDY0Vjc2SDY4VjUyWiIgZmlsbD0id2hpdGUiLz4KPHBhdGggZD0iTTQwIDQ4SDI0IiBzdHJva2U9IiNGRTk1NDUiIHN0cm9rZS13aWR0aD0iMyIgc3Ryb2tlLWxpbmVjYXA9InJvdW5kIi8+CjxwYXRoIGQ9Ik0xMDAgNDhINjgiIHN0cm9rZT0iI0ZFOTU0NSIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPC9zdmc+Cg==',
          is_active: true,
          created_at: new Date().toISOString()
        },
        {
          id: 'master_service_10',
          title: 'Lavagem Completa',
          description: 'Lavagem externa e interna do veículo',
          category: 'Estética',
          estimated_duration: 60,
          base_price: 5000,
          image_url: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTIwIiBoZWlnaHQ9IjEyMCIgdmlld0JveD0iMCAwIDEyMCAxMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIxMjAiIGhlaWdodD0iMTIwIiByeD0iMjQiIGZpbGw9InVybCgjZ3JhZGllbnQxMCkvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudDEwIiB4MT0iMCIgeTE9IjAiIHgyPSIxMjAiIHkyPSIxMjAiIGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIj4KPHN0b3Agc3RvcC1jb2xvcj0iIzA2QjZBNyIvPgo8c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiMwODkxOTEiLz4KPC9saW5lYXJHcmFkaWVudD4KPC9kZWZzPgo8cGF0aCBkPSJNNjAgMjBDNDAuMiAyMCAyNCAzNi4yIDI0IDU2VjEwNEMyNCAxMjMuOCA0MC4yIDE0MCA2MCAxNDBDNzkuOCAxNDAgOTYgMTIzLjggOTYgMTA0VjU2Qzk2IDM2LjIgNzkuOCAyMCA2MCAyMFoiIGZpbGw9IndoaXRlIiBmaWxsLW9wYWNpdHk9IjAuMSIvPgo8cGF0aCBkPSJNNjAgMjhDNDQuNTMgMjggMzIgNDAuNTMgMzIgNTZWMTAwQzMyIDExNS40NyA0NC41MyAxMjggNjAgMTI4Qzc1LjQ3IDEyOCA4OCAxMTUuNDcgODggMTAwVjU2Qzg4IDQwLjUzIDc1LjQ3IDI4IDYwIDI4WiIgZmlsbD0id2hpdGUiIGZpbGwtb3BhY2l0eT0iMC4yIi8+CjxjaXJjbGUgY3g9IjYwIiBjeT0iNjQiIHI9IjE2IiBmaWxsPSIjMDZCNkE3Ii8vPgo8Y2lyY2xlIGN4PSI2MCIgY3k9IjY0IiByPSI4IiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTYgNTZINDRWNzJINTZWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNTIgNTJINDhWNzZINTJWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNjggNTJINjRWNzZINjhWNjBaIiBmaWxsPSJ3aGl0ZSIvPgo8cGF0aCBkPSJNNDAgNDhIMjQiIHN0cm9rZT0iIzA2QjZBNyIgc3Ryb2tlLXdpZHRoPSIzIiBzdHJva2UtbGluZWNhcD0icm91bmQiLz4KPHBhdGggZD0iTTEwMCA0OEg2OCIgc3Ryb2tlPSIjMDZCNkE3IiBzdHJva2Utd2lkdGg9IjMiIHN0cm9rZS1saW5lY2FwPSJyb3VuZCIvPgo8L3N2Zz4K',
          is_active: true,
          created_at: new Date().toISOString()
        }
      ]
      
      return res.json({
        success: true,
        data: {
          services: defaultServices,
          count: defaultServices.length
        }
      })
    }
    
    // Formatar serviços padrão
    const formattedServices = masterServices.map(service => ({
      id: service.id,
      title: service.title,
      description: service.description,
      category: service.metadata?.category || 'Geral',
      estimated_duration: service.metadata?.estimated_duration || 60,
      base_price: service.variants?.[0]?.prices?.[0]?.amount || 0,
      image_url: service.thumbnail,
      is_active: true,
      created_at: service.created_at
    }))
    
    return res.json({
      success: true,
      data: {
        services: formattedServices,
        count: formattedServices.length
      }
    })
    
  } catch (error) {
    console.error("Erro ao buscar serviços padrão:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao buscar serviços padrão",
      error: error.message
    })
  }
}

/**
 * POST /admin/master-services
 * 
 * Criar um novo serviço padrão
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const productModuleService = req.scope.resolve(Modules.PRODUCT)
  
  const {
    title,
    description,
    category,
    estimated_duration,
    base_price,
    image_url
  } = req.body
  
  // Validações
  if (!title || !description || !base_price) {
    return res.status(400).json({
      message: "title, description e base_price são obrigatórios"
    })
  }
  
  try {
    // Criar produto como serviço padrão
    const product = await productModuleService.createProducts({
      title,
      description,
      is_giftcard: false,
      discountable: true,
      thumbnail: image_url,
      metadata: {
        is_master_service: true,
        category: category || 'Geral',
        estimated_duration: estimated_duration || 60
      },
      variants: [
        {
          title: "Padrão",
          prices: [
            {
              amount: base_price,
              currency_code: "brl",
            }
          ],
          manage_inventory: false,
        }
      ]
    })
    
    return res.status(201).json({
      success: true,
      message: "Serviço padrão criado com sucesso",
      data: {
        id: product.id,
        title: product.title,
        description: product.description,
        category: product.metadata?.category,
        estimated_duration: product.metadata?.estimated_duration,
        base_price: product.variants?.[0]?.prices?.[0]?.amount,
        image_url: product.thumbnail,
        is_active: true,
        created_at: product.created_at
      }
    })
    
  } catch (error) {
    console.error("Erro ao criar serviço padrão:", error)
    
    return res.status(500).json({
      success: false,
      message: "Erro ao criar serviço padrão",
      error: error.message
    })
  }
}