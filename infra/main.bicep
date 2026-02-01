// =============================================================================
// Main Bicep Template - Event-Driven Bookstore Notification System
// Orchestrates deployment of all Azure resources
// =============================================================================

targetScope = 'subscription'

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Environment to deploy (dev, test, stg, prod)')
@allowed(['dev', 'test', 'stg', 'prod'])
param environment string

@description('Azure region for deployment')
param location string = 'eastus'

// Cosmos DB Configuration
@description('Cosmos DB configuration')
param cosmosDbConfig object

// Service Bus Configuration
@description('Service Bus configuration')
param serviceBusConfig object

// API Management Configuration
@description('API Management configuration')
param apimConfig object

// Function App Configuration
@description('Function App configuration')
param functionAppConfig object

// Key Vault Configuration
@description('Key Vault configuration')
param keyVaultConfig object

// Monitoring Configuration
@description('Monitoring configuration')
param monitoringConfig object

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var deploymentTimestamp = utcNow('yyyyMMddHHmmss')

// -----------------------------------------------------------------------------
// Module: Naming Convention
// -----------------------------------------------------------------------------

module naming 'modules/naming.bicep' = {
  name: 'naming-${deploymentTimestamp}'
  params: {
    environment: environment
    location: location
  }
}

// -----------------------------------------------------------------------------
// Module: Tags
// -----------------------------------------------------------------------------

module tags 'modules/tags.bicep' = {
  name: 'tags-${deploymentTimestamp}'
  params: {
    environment: environment
  }
}

// -----------------------------------------------------------------------------
// Resource Group
// -----------------------------------------------------------------------------

resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: naming.outputs.resourceGroup
  location: location
  tags: tags.outputs.tags
}

// -----------------------------------------------------------------------------
// Module 04: Security & Identity
// -----------------------------------------------------------------------------

// User-Assigned Managed Identity
module managedIdentity 'modules/managed-identity.bicep' = {
  name: 'managed-identity-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    identityName: naming.outputs.managedIdentity
    location: location
    tags: tags.outputs.tags
  }
}

// Key Vault for secrets management
module keyVault 'modules/keyvault.bicep' = {
  name: 'keyvault-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    keyVaultName: naming.outputs.keyVault
    location: location
    tags: tags.outputs.tags
    sku: keyVaultConfig.sku
    enableSoftDelete: keyVaultConfig.enableSoftDelete
    softDeleteRetentionDays: keyVaultConfig.softDeleteRetentionDays
    enablePurgeProtection: keyVaultConfig.enablePurgeProtection
    enableRbacAuthorization: true
    secretsUserPrincipalIds: [managedIdentity.outputs.principalId]
  }
  dependsOn: [managedIdentity]
}

// -----------------------------------------------------------------------------
// Module 05: Cosmos DB Data Layer
// -----------------------------------------------------------------------------

module cosmosDb 'modules/cosmosdb.bicep' = {
  name: 'cosmosdb-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    accountName: naming.outputs.cosmosAccount
    location: location
    tags: tags.outputs.tags
    databaseName: naming.outputs.cosmosDatabase
    serverless: cosmosDbConfig.serverless
    throughput: cosmosDbConfig.throughput
    enableAutomaticFailover: cosmosDbConfig.enableAutomaticFailover
    enableMultipleWriteLocations: cosmosDbConfig.enableMultipleWriteLocations
    backupPolicy: cosmosDbConfig.backupPolicy
    dataContributorPrincipalIds: [managedIdentity.outputs.principalId]
  }
  dependsOn: [managedIdentity]
}

// -----------------------------------------------------------------------------
// Module 06: API Management
// -----------------------------------------------------------------------------

module apim 'modules/apim.bicep' = {
  name: 'apim-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    apimName: naming.outputs.apim
    location: location
    tags: tags.outputs.tags
    sku: apimConfig.sku
    capacity: apimConfig.capacity
    publisherEmail: apimConfig.publisherEmail
    publisherName: apimConfig.publisherName
  }
}

// -----------------------------------------------------------------------------
// Module 07: Event Grid & Storage
// -----------------------------------------------------------------------------

// Storage Account (for Function Apps and dead-letter)
module storage 'modules/storage.bicep' = {
  name: 'storage-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    storageAccountName: naming.outputs.storageAccount
    location: location
    tags: tags.outputs.tags
    sku: 'Standard_LRS'
    createDeadLetterContainer: true
    blobContributorPrincipalIds: [managedIdentity.outputs.principalId]
  }
  dependsOn: [managedIdentity]
}

// Event Grid Topic
module eventGrid 'modules/eventgrid.bicep' = {
  name: 'eventgrid-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    topicName: naming.outputs.eventGridTopic
    location: location
    tags: tags.outputs.tags
    inputSchema: 'CloudEventSchemaV1_0'
  }
}

// -----------------------------------------------------------------------------
// Module 08: Service Bus Messaging
// -----------------------------------------------------------------------------

// Service Bus Namespace and Queue
module serviceBus 'modules/servicebus.bicep' = {
  name: 'servicebus-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    namespaceName: naming.outputs.serviceBusNamespace
    location: location
    tags: tags.outputs.tags
    sku: serviceBusConfig.sku
    messageRetentionDays: serviceBusConfig.messageRetentionDays
    dataSenderPrincipalIds: [
      managedIdentity.outputs.principalId
      eventGrid.outputs.identityPrincipalId
    ]
    dataReceiverPrincipalIds: [managedIdentity.outputs.principalId]
  }
  dependsOn: [managedIdentity, eventGrid]
}

// Event Grid Subscription to Service Bus
module eventGridSubscription 'modules/eventgrid-subscription.bicep' = {
  name: 'eventgrid-subscription-${deploymentTimestamp}'
  scope: resourceGroup
  params: {
    eventGridTopicName: eventGrid.outputs.name
    subscriptionName: naming.outputs.eventGridSubscription
    serviceBusQueueId: serviceBus.outputs.notificationsQueueId
    deadLetterDestination: storage.outputs.id
    includedEventTypes: [
      'com.bookstore.book.created'
    ]
    maxDeliveryAttempts: 10
    eventTimeToLiveInMinutes: 1440
  }
  dependsOn: [eventGrid, serviceBus, storage]
}

// -----------------------------------------------------------------------------
// Future Module Deployments
// These will be added in subsequent modules
// -----------------------------------------------------------------------------

// Module 09-11: Function Apps
// module functionApp 'modules/functionapp.bicep' = { ... }

// Module 12: Monitoring
// module monitoring 'modules/monitoring.bicep' = { ... }

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Resource group name')
output resourceGroupName string = resourceGroup.name

@description('Resource group ID')
output resourceGroupId string = resourceGroup.id

@description('Environment deployed')
output environment string = environment

@description('Region deployed')
output location string = location

@description('Deployment timestamp')
output deploymentTimestamp string = deploymentTimestamp

// Resource Names (from naming module)
@description('All generated resource names')
output resourceNames object = {
  resourceGroup: naming.outputs.resourceGroup
  functionApp: naming.outputs.functionApp
  appServicePlan: naming.outputs.appServicePlan
  cosmosAccount: naming.outputs.cosmosAccount
  cosmosDatabase: naming.outputs.cosmosDatabase
  serviceBusNamespace: naming.outputs.serviceBusNamespace
  eventGridTopic: naming.outputs.eventGridTopic
  keyVault: naming.outputs.keyVault
  storageAccount: naming.outputs.storageAccount
  appInsights: naming.outputs.appInsights
  logAnalytics: naming.outputs.logAnalytics
  apim: naming.outputs.apim
}

// Tags applied
@description('Tags applied to resources')
output appliedTags object = tags.outputs.tags

// Security outputs (Module 04)
@description('Managed Identity details')
output managedIdentity object = {
  id: managedIdentity.outputs.id
  name: managedIdentity.outputs.name
  principalId: managedIdentity.outputs.principalId
  clientId: managedIdentity.outputs.clientId
}

@description('Key Vault details')
output keyVault object = {
  id: keyVault.outputs.id
  name: keyVault.outputs.name
  uri: keyVault.outputs.vaultUri
}

// Cosmos DB outputs (Module 05)
@description('Cosmos DB details')
output cosmosDb object = {
  id: cosmosDb.outputs.id
  name: cosmosDb.outputs.name
  endpoint: cosmosDb.outputs.endpoint
  databaseName: cosmosDb.outputs.databaseName
  containerNames: cosmosDb.outputs.containerNames
}

// API Management outputs (Module 06)
@description('API Management details')
output apim object = {
  id: apim.outputs.id
  name: apim.outputs.name
  gatewayUrl: apim.outputs.gatewayUrl
}

// Storage Account outputs (Module 07)
@description('Storage Account details')
output storage object = {
  id: storage.outputs.id
  name: storage.outputs.name
  primaryEndpoint: storage.outputs.primaryEndpoint
}

// Event Grid outputs (Module 07)
@description('Event Grid details')
output eventGrid object = {
  id: eventGrid.outputs.id
  name: eventGrid.outputs.name
  endpoint: eventGrid.outputs.endpoint
}

// Service Bus outputs (Module 08)
@description('Service Bus details')
output serviceBus object = {
  id: serviceBus.outputs.id
  name: serviceBus.outputs.name
  endpoint: serviceBus.outputs.serviceBusEndpoint
  notificationsQueueName: serviceBus.outputs.notificationsQueueName
}
