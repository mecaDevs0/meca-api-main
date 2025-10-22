export default async function createKey({ container }) {
  try {
    // Tentar diferentes nomes de serviço
    const serviceNames = [
      'publishableApiKeyService',
      'publishableApiKey',
      'publishableApiKeyModuleService',
      'publishableApiKeyService'
    ]
    
    for (const serviceName of serviceNames) {
      try {
        const service = container.resolve(serviceName)
        const key = await service.createPublishableApiKeys({
          title: 'MECA Store Key',
          type: 'publishable'
        })
        console.log('✅ Publishable API Key created:', key.token)
        return key
      } catch (e) {
        console.log(`❌ ${serviceName}: ${e.message}`)
      }
    }
    
    console.log('❌ No service found')
    return null
  } catch (error) {
    console.log('❌ Error creating key:', error.message)
    return null
  }
}
