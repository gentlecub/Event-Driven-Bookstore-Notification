// =============================================================================
// Event Grid Subscription Module
// Routes events from Event Grid Topic to Service Bus
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Event Grid Topic name (parent resource)')
param eventGridTopicName string

@description('Subscription name')
param subscriptionName string

@description('Service Bus Queue resource ID (destination)')
param serviceBusQueueId string

@description('Dead-letter storage container URL')
param deadLetterDestination string = ''

@description('Event types to subscribe to (empty = all)')
param includedEventTypes array = []

@description('Subject filter - begins with')
param subjectBeginsWith string = ''

@description('Subject filter - ends with')
param subjectEndsWith string = ''

@description('Maximum delivery attempts before dead-letter')
param maxDeliveryAttempts int = 30

@description('Event time to live in minutes')
param eventTimeToLiveInMinutes int = 1440

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var hasDeadLetter = !empty(deadLetterDestination)
var hasEventTypeFilter = !empty(includedEventTypes)
var hasSubjectFilter = !empty(subjectBeginsWith) || !empty(subjectEndsWith)

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// Reference existing Event Grid Topic
resource eventGridTopic 'Microsoft.EventGrid/topics@2023-12-15-preview' existing = {
  name: eventGridTopicName
}

// Event Grid Subscription
resource eventGridSubscription 'Microsoft.EventGrid/topics/eventSubscriptions@2023-12-15-preview' = {
  parent: eventGridTopic
  name: subscriptionName
  properties: {
    destination: {
      endpointType: 'ServiceBusQueue'
      properties: {
        resourceId: serviceBusQueueId
      }
    }
    filter: {
      includedEventTypes: hasEventTypeFilter ? includedEventTypes : null
      subjectBeginsWith: hasSubjectFilter ? subjectBeginsWith : ''
      subjectEndsWith: hasSubjectFilter ? subjectEndsWith : ''
      enableAdvancedFilteringOnArrays: true
    }
    retryPolicy: {
      maxDeliveryAttempts: maxDeliveryAttempts
      eventTimeToLiveInMinutes: eventTimeToLiveInMinutes
    }
    deadLetterDestination: hasDeadLetter ? {
      endpointType: 'StorageBlob'
      properties: {
        resourceId: split(deadLetterDestination, '/blobServices')[0]
        blobContainerName: 'deadletter'
      }
    } : null
    eventDeliverySchema: 'CloudEventSchemaV1_0'
  }
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Event Grid Subscription resource ID')
output id string = eventGridSubscription.id

@description('Event Grid Subscription name')
output name string = eventGridSubscription.name

@description('Event Grid Subscription provisioning state')
output provisioningState string = eventGridSubscription.properties.provisioningState
