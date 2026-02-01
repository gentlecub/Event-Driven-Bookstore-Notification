// =============================================================================
// Service Bus Module
// Provides reliable messaging for notification processing
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Service Bus namespace name')
param namespaceName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

@description('Service Bus SKU')
@allowed(['Basic', 'Standard', 'Premium'])
param sku string = 'Standard'

@description('Capacity units (Premium only, 1-16)')
param capacity int = 0

@description('Enable zone redundancy (Premium only)')
param zoneRedundant bool = false

@description('Message retention in days')
@minValue(1)
@maxValue(14)
param messageRetentionDays int = 1

@description('Principal IDs to grant Service Bus Data Sender role')
param dataSenderPrincipalIds array = []

@description('Principal IDs to grant Service Bus Data Receiver role')
param dataReceiverPrincipalIds array = []

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var isPremium = sku == 'Premium'
var isBasic = sku == 'Basic'

// Queue definitions
var queues = [
  {
    name: 'notifications'
    maxDeliveryCount: 10
    lockDuration: 'PT5M'  // 5 minutes
    defaultMessageTimeToLive: 'P${messageRetentionDays}D'
    deadLetteringOnMessageExpiration: true
    requiresDuplicateDetection: !isBasic
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    requiresSession: false
  }
]

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  tags: tags
  sku: {
    name: sku
    tier: sku
    capacity: isPremium ? capacity : 0
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    zoneRedundant: isPremium ? zoneRedundant : false
    disableLocalAuth: false  // Set to true for enhanced security
  }
}

// Queues
resource serviceBusQueues 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = [for queue in queues: {
  parent: serviceBusNamespace
  name: queue.name
  properties: {
    maxDeliveryCount: queue.maxDeliveryCount
    lockDuration: queue.lockDuration
    defaultMessageTimeToLive: queue.defaultMessageTimeToLive
    deadLetteringOnMessageExpiration: queue.deadLetteringOnMessageExpiration
    requiresDuplicateDetection: queue.requiresDuplicateDetection
    duplicateDetectionHistoryTimeWindow: queue.requiresDuplicateDetection ? queue.duplicateDetectionHistoryTimeWindow : null
    requiresSession: queue.requiresSession
    maxSizeInMegabytes: 1024
    enablePartitioning: false
    enableBatching: true
  }
}]

// -----------------------------------------------------------------------------
// RBAC Role Assignments
// -----------------------------------------------------------------------------

// Service Bus Data Sender role
var dataSenderRoleId = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'

resource dataSenderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in dataSenderPrincipalIds: {
  name: guid(serviceBusNamespace.id, principalId, dataSenderRoleId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', dataSenderRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]

// Service Bus Data Receiver role
var dataReceiverRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'

resource dataReceiverAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in dataReceiverPrincipalIds: {
  name: guid(serviceBusNamespace.id, principalId, dataReceiverRoleId)
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', dataReceiverRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Service Bus namespace resource ID')
output id string = serviceBusNamespace.id

@description('Service Bus namespace name')
output name string = serviceBusNamespace.name

@description('Service Bus namespace FQDN')
output serviceBusEndpoint string = serviceBusNamespace.properties.serviceBusEndpoint

@description('Service Bus namespace identity principal ID')
output identityPrincipalId string = serviceBusNamespace.identity.principalId

@description('Notifications queue resource ID')
output notificationsQueueId string = serviceBusQueues[0].id

@description('Notifications queue name')
output notificationsQueueName string = serviceBusQueues[0].name

@description('Service Bus resource')
output resource object = {
  id: serviceBusNamespace.id
  name: serviceBusNamespace.name
  endpoint: serviceBusNamespace.properties.serviceBusEndpoint
  notificationsQueueId: serviceBusQueues[0].id
  notificationsQueueName: serviceBusQueues[0].name
}

// Connection string (for legacy scenarios - prefer Managed Identity)
@description('Primary connection string (use Managed Identity instead when possible)')
output connectionString string = listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBusNamespace.apiVersion).primaryConnectionString
