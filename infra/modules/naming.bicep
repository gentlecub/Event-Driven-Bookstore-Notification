// =============================================================================
// Naming Convention Module
// Generates consistent resource names following CAF naming standards
// Pattern: {resource-type}-{workload}-{environment}-{region}-{instance}
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Environment name (dev, test, stg, prod)')
@allowed(['dev', 'test', 'stg', 'prod'])
param environment string

@description('Azure region for deployment')
param location string

@description('Workload/project name')
param workload string = 'bookstore'

@description('Instance number for uniqueness')
param instance string = '001'

// -----------------------------------------------------------------------------
// Variables - Region Abbreviations
// -----------------------------------------------------------------------------

var regionAbbreviations = {
  eastus: 'eus'
  eastus2: 'eus2'
  westus: 'wus'
  westus2: 'wus2'
  centralus: 'cus'
  northeurope: 'neu'
  westeurope: 'weu'
  uksouth: 'uks'
  southeastasia: 'sea'
  australiaeast: 'aue'
  japaneast: 'jpe'
  brazilsouth: 'brs'
}

var regionAbbr = contains(regionAbbreviations, location)
  ? regionAbbreviations[location]
  : substring(location, 0, 3)

// -----------------------------------------------------------------------------
// Variables - Base Naming Components
// -----------------------------------------------------------------------------

var baseName = '${workload}-${environment}-${regionAbbr}'
var baseNameWithInstance = '${baseName}-${instance}'

// -----------------------------------------------------------------------------
// Resource Names - Standard Pattern
// {type}-{workload}-{env}-{region}-{instance}
// -----------------------------------------------------------------------------

// Resource Group
var resourceGroupName = 'rg-${baseNameWithInstance}'

// Compute & Functions
var functionAppName = 'func-${baseNameWithInstance}'
var appServicePlanName = 'asp-${baseNameWithInstance}'

// API Management
var apimName = 'apim-${baseName}'

// Cosmos DB (no instance, globally unique)
var cosmosAccountName = 'cosmos-${baseName}'
var cosmosDatabaseName = '${workload}-db'

// Service Bus
var serviceBusNamespaceName = 'sb-${baseName}'

// Event Grid
var eventGridTopicName = 'evgt-${baseName}'

// Key Vault (max 24 chars, globally unique)
var keyVaultName = take('kv-${baseName}', 24)

// Storage Account (lowercase, no hyphens, max 24 chars)
var storageAccountName = take(toLower(replace('st${workload}${environment}${regionAbbr}${instance}', '-', '')), 24)

// Monitoring
var appInsightsName = 'appi-${baseNameWithInstance}'
var logAnalyticsName = 'log-${baseNameWithInstance}'

// Managed Identity
var managedIdentityName = 'id-${baseName}-func'

// -----------------------------------------------------------------------------
// Child Resource Names (within parent resources)
// -----------------------------------------------------------------------------

// Cosmos DB containers
var cosmosContainerBooks = 'books'
var cosmosContainerSubscribers = 'subscribers'

// Service Bus
var serviceBusQueueNotifications = 'notifications'
var serviceBusTopicBookEvents = 'book-events'
var serviceBusSubscriptionHandler = 'notification-handler'

// Event Grid
var eventGridSubscriptionName = 'evgs-sb-forwarder'

// -----------------------------------------------------------------------------
// Outputs - Resource Names
// -----------------------------------------------------------------------------

@description('Resource group name')
output resourceGroup string = resourceGroupName

@description('Function App name')
output functionApp string = functionAppName

@description('App Service Plan name')
output appServicePlan string = appServicePlanName

@description('API Management name')
output apim string = apimName

@description('Cosmos DB account name')
output cosmosAccount string = cosmosAccountName

@description('Cosmos DB database name')
output cosmosDatabase string = cosmosDatabaseName

@description('Cosmos DB books container name')
output cosmosContainerBooks string = cosmosContainerBooks

@description('Cosmos DB subscribers container name')
output cosmosContainerSubscribers string = cosmosContainerSubscribers

@description('Service Bus namespace name')
output serviceBusNamespace string = serviceBusNamespaceName

@description('Service Bus notifications queue name')
output serviceBusQueue string = serviceBusQueueNotifications

@description('Service Bus book events topic name')
output serviceBusTopic string = serviceBusTopicBookEvents

@description('Service Bus subscription handler name')
output serviceBusSubscription string = serviceBusSubscriptionHandler

@description('Event Grid topic name')
output eventGridTopic string = eventGridTopicName

@description('Event Grid subscription name')
output eventGridSubscription string = eventGridSubscriptionName

@description('Key Vault name')
output keyVault string = keyVaultName

@description('Storage account name')
output storageAccount string = storageAccountName

@description('Application Insights name')
output appInsights string = appInsightsName

@description('Log Analytics workspace name')
output logAnalytics string = logAnalyticsName

@description('Managed Identity name')
output managedIdentity string = managedIdentityName

// -----------------------------------------------------------------------------
// Outputs - Computed Values
// -----------------------------------------------------------------------------

@description('Region abbreviation used in naming')
output regionAbbreviation string = regionAbbr

@description('Base name without instance (for resources that must be globally unique)')
output baseName string = baseName

@description('Base name with instance')
output baseNameWithInstance string = baseNameWithInstance
