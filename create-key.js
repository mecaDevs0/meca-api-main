import { Modules } from '@medusajs/framework/utils'

export default async function createKey({ container }) {
  try {
    const publishableApiKeyService = container.resolve('publishableApiKeyService')
    
    const key = await publishableApiKeyService.createPublishableApiKeys({
      title: 'MECA Store Key',
      type: 'publishable'
    })
    
    console.log('✅ Publishable API Key created:', key.token)
    return key
  } catch (error) {
    console.log('❌ Error creating key:', error.message)
    return null
  }
}
