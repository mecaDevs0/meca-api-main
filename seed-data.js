export default async function seedData({ container }) {
  try {
    console.log('🌱 Iniciando seed de dados...')
    
    // Criar região
    const regionService = container.resolve('regionService')
    const region = await regionService.createRegions({
      name: 'Brasil',
      currency_code: 'brl',
      tax_rate: 0.1
    })
    console.log('✅ Região criada:', region.id)
    
    // Criar produtos
    const productService = container.resolve('productService')
    const product = await productService.createProducts({
      title: 'Serviço de Manutenção',
      handle: 'servico-manutencao',
      description: 'Serviço completo de manutenção automotiva',
      status: 'published',
      variants: [{
        title: 'Padrão',
        sku: 'SERV-001',
        prices: [{
          amount: 15000, // R$ 150,00
          currency_code: 'brl'
        }]
      }]
    })
    console.log('✅ Produto criado:', product.id)
    
    // Criar coleção
    const collectionService = container.resolve('collectionService')
    const collection = await collectionService.createCollections({
      title: 'Serviços MECA',
      handle: 'servicos-meca'
    })
    console.log('✅ Coleção criada:', collection.id)
    
    console.log('🎉 Seed concluído com sucesso!')
    return true
  } catch (error) {
    console.log('❌ Erro no seed:', error.message)
    return false
  }
}
