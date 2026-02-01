// =============================================================================
// Cosmos DB Module
// Provides NoSQL database for books and subscribers
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Cosmos DB account name')
param accountName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

@description('Database name')
param databaseName string

@description('Enable serverless capacity mode')
param serverless bool = true

@description('Provisioned throughput (RU/s) - ignored if serverless')
param throughput int = 400

@description('Enable automatic failover')
param enableAutomaticFailover bool = false

@description('Enable multi-region writes')
param enableMultipleWriteLocations bool = false

@description('Backup policy type')
@allowed(['Continuous', 'Periodic'])
param backupPolicy string = 'Periodic'

@description('Principal IDs to grant Cosmos DB Data Contributor role')
param dataContributorPrincipalIds array = []

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var locations = [
  {
    locationName: location
    failoverPriority: 0
    isZoneRedundant: false
  }
]

var consistencyPolicy = {
  defaultConsistencyLevel: 'Session'
  maxStalenessPrefix: 100
  maxIntervalInSeconds: 5
}

var backupPolicyConfig = backupPolicy == 'Continuous' ? {
  type: 'Continuous'
  continuousModeProperties: {
    tier: 'Continuous7Days'
  }
} : {
  type: 'Periodic'
  periodicModeProperties: {
    backupIntervalInMinutes: 240
    backupRetentionIntervalInHours: 8
    backupStorageRedundancy: 'Local'
  }
}

// Container definitions
var containers = [
  {
    name: 'books'
    partitionKey: '/category'
    indexingPolicy: {
      indexingMode: 'consistent'
      automatic: true
      includedPaths: [
        { path: '/*' }
      ]
      excludedPaths: [
        { path: '/"_etag"/?' }
      ]
    }
    uniqueKeyPolicy: {
      uniqueKeys: [
        { paths: ['/isbn'] }
      ]
    }
  }
  {
    name: 'subscribers'
    partitionKey: '/id'
    indexingPolicy: {
      indexingMode: 'consistent'
      automatic: true
      includedPaths: [
        { path: '/email/?' }
        { path: '/isActive/?' }
        { path: '/subscribedCategories/*' }
      ]
      excludedPaths: [
        { path: '/*' }
        { path: '/"_etag"/?' }
      ]
    }
    uniqueKeyPolicy: {
      uniqueKeys: [
        { paths: ['/email'] }
      ]
    }
  }
]

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// Cosmos DB Account
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: accountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: locations
    consistencyPolicy: consistencyPolicy
    enableAutomaticFailover: enableAutomaticFailover
    enableMultipleWriteLocations: enableMultipleWriteLocations
    backupPolicy: backupPolicyConfig
    capabilities: serverless ? [
      { name: 'EnableServerless' }
    ] : []
    // Security settings
    disableLocalAuth: false  // Set to true to enforce Azure AD only
    publicNetworkAccess: 'Enabled'  // Use Private Endpoints in production
    enableAnalyticalStorage: false
  }
}

// Database
resource database 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  parent: cosmosAccount
  name: databaseName
  properties: {
    resource: {
      id: databaseName
    }
    options: serverless ? {} : {
      throughput: throughput
    }
  }
}

// Containers
resource cosmosContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-11-15' = [for container in containers: {
  parent: database
  name: container.name
  properties: {
    resource: {
      id: container.name
      partitionKey: {
        paths: [container.partitionKey]
        kind: 'Hash'
        version: 2
      }
      indexingPolicy: container.indexingPolicy
      uniqueKeyPolicy: container.uniqueKeyPolicy
    }
    options: serverless ? {} : {
      throughput: throughput
    }
  }
}]

// -----------------------------------------------------------------------------
// RBAC Role Assignments
// -----------------------------------------------------------------------------

// Cosmos DB Data Contributor role
// Allows read/write access to data plane operations
var dataContributorRoleId = '00000000-0000-0000-0000-000000000002'

resource dataContributorAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2023-11-15' = [for (principalId, i) in dataContributorPrincipalIds: {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, principalId, dataContributorRoleId)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${dataContributorRoleId}'
    principalId: principalId
    scope: cosmosAccount.id
  }
}]

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Cosmos DB account ID')
output id string = cosmosAccount.id

@description('Cosmos DB account name')
output name string = cosmosAccount.name

@description('Cosmos DB account endpoint')
output endpoint string = cosmosAccount.properties.documentEndpoint

@description('Database name')
output databaseName string = database.name

@description('Container names')
output containerNames array = [for (container, i) in containers: cosmosContainers[i].name]

@description('Cosmos DB resource for reference')
output resource object = {
  id: cosmosAccount.id
  name: cosmosAccount.name
  endpoint: cosmosAccount.properties.documentEndpoint
  databaseName: database.name
}

// Connection string (for legacy scenarios - prefer Managed Identity)
@description('Primary connection string (use Managed Identity instead when possible)')
output connectionString string = cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
