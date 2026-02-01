// =============================================================================
// Event Grid Module
// Provides event routing for the Bookstore Notification System
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Event Grid Topic name')
param topicName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

@description('Enable local authentication (disable for enhanced security)')
param disableLocalAuth bool = false

@description('Public network access')
@allowed(['Enabled', 'Disabled'])
param publicNetworkAccess string = 'Enabled'

@description('Input schema for events')
@allowed(['EventGridSchema', 'CloudEventSchemaV1_0', 'CustomEventSchema'])
param inputSchema string = 'CloudEventSchemaV1_0'

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// Event Grid Topic
resource eventGridTopic 'Microsoft.EventGrid/topics@2023-12-15-preview' = {
  name: topicName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    inputSchema: inputSchema
    publicNetworkAccess: publicNetworkAccess
    disableLocalAuth: disableLocalAuth
    dataResidencyBoundary: 'WithinGeopair'
  }
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Event Grid Topic resource ID')
output id string = eventGridTopic.id

@description('Event Grid Topic name')
output name string = eventGridTopic.name

@description('Event Grid Topic endpoint')
output endpoint string = eventGridTopic.properties.endpoint

@description('Event Grid Topic identity principal ID')
output identityPrincipalId string = eventGridTopic.identity.principalId

@description('Event Grid Topic resource')
output resource object = {
  id: eventGridTopic.id
  name: eventGridTopic.name
  endpoint: eventGridTopic.properties.endpoint
  identityPrincipalId: eventGridTopic.identity.principalId
}
