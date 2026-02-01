// =============================================================================
// Storage Account Module
// Provides storage for Function Apps and Event Grid dead-letter
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Storage account name (lowercase, no hyphens, max 24 chars)')
param storageAccountName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

@description('Storage account SKU')
@allowed(['Standard_LRS', 'Standard_GRS', 'Standard_RAGRS', 'Standard_ZRS', 'Premium_LRS'])
param sku string = 'Standard_LRS'

@description('Storage account kind')
@allowed(['StorageV2', 'BlobStorage', 'BlockBlobStorage'])
param kind string = 'StorageV2'

@description('Enable blob public access')
param allowBlobPublicAccess bool = false

@description('Minimum TLS version')
@allowed(['TLS1_0', 'TLS1_1', 'TLS1_2'])
param minimumTlsVersion string = 'TLS1_2'

@description('Enable hierarchical namespace (Data Lake Gen2)')
param enableHierarchicalNamespace bool = false

@description('Create dead-letter container for Event Grid')
param createDeadLetterContainer bool = true

@description('Principal IDs to grant Storage Blob Data Contributor role')
param blobContributorPrincipalIds array = []

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  kind: kind
  sku: {
    name: sku
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: allowBlobPublicAccess
    allowSharedKeyAccess: true  // Required for Function Apps
    minimumTlsVersion: minimumTlsVersion
    supportsHttpsTrafficOnly: true
    isHnsEnabled: enableHierarchicalNamespace
    encryption: {
      services: {
        blob: { enabled: true }
        file: { enabled: true }
        queue: { enabled: true }
        table: { enabled: true }
      }
      keySource: 'Microsoft.Storage'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Blob Service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
    containerDeleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

// Dead-letter Container for Event Grid
resource deadLetterContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = if (createDeadLetterContainer) {
  parent: blobService
  name: 'deadletter'
  properties: {
    publicAccess: 'None'
  }
}

// Function App Containers
resource functionAppContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'azure-webjobs-hosts'
  properties: {
    publicAccess: 'None'
  }
}

resource functionAppSecretsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'azure-webjobs-secrets'
  properties: {
    publicAccess: 'None'
  }
}

// -----------------------------------------------------------------------------
// RBAC Role Assignments
// -----------------------------------------------------------------------------

// Storage Blob Data Contributor role
var blobContributorRoleId = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource blobContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in blobContributorPrincipalIds: {
  name: guid(storageAccount.id, principalId, blobContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', blobContributorRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Storage account resource ID')
output id string = storageAccount.id

@description('Storage account name')
output name string = storageAccount.name

@description('Storage account primary endpoint')
output primaryEndpoint string = storageAccount.properties.primaryEndpoints.blob

@description('Storage account primary connection string')
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'

@description('Dead-letter container URL')
output deadLetterContainerUrl string = createDeadLetterContainer ? '${storageAccount.properties.primaryEndpoints.blob}deadletter' : ''

@description('Storage account resource')
output resource object = {
  id: storageAccount.id
  name: storageAccount.name
  primaryEndpoint: storageAccount.properties.primaryEndpoints.blob
}
