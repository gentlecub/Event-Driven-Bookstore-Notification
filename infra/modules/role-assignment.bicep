// =============================================================================
// Role Assignment Module
// Provides reusable RBAC role assignments
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Principal ID (object ID) to assign the role to')
param principalId string

@description('Role definition ID or built-in role name')
param roleDefinitionId string

@description('Principal type')
@allowed(['User', 'Group', 'ServicePrincipal', 'ForeignGroup'])
param principalType string = 'ServicePrincipal'

@description('Description of the role assignment')
param description string = ''

// -----------------------------------------------------------------------------
// Built-in Role Definition IDs
// Reference: https://docs.microsoft.com/azure/role-based-access-control/built-in-roles
// -----------------------------------------------------------------------------

var builtInRoles = {
  // General
  Owner: '8e3af657-a8ff-443c-a75c-2fe8c4bcb635'
  Contributor: 'b24988ac-6180-42a0-ab88-20f7382dd24c'
  Reader: 'acdd72a7-3385-48ef-bd42-f606fba81ae7'

  // Key Vault
  KeyVaultAdministrator: '00482a5a-887f-4fb3-b363-3b7fe8e74483'
  KeyVaultSecretsOfficer: 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'
  KeyVaultSecretsUser: '4633458b-17de-408a-b874-0445c86b69e6'
  KeyVaultCertificatesOfficer: 'a4417e6f-fecd-4de8-b567-7b0420556985'
  KeyVaultCryptoOfficer: '14b46e9e-c2b7-41b4-b07b-48a6ebf60603'

  // Cosmos DB
  CosmosDBAccountReader: 'fbdf93bf-df7d-467e-a4d2-9458aa1360c8'
  CosmosDBOperator: '230815da-be43-4aae-9cb4-875f7bd000aa'
  DocumentDBAccountContributor: '5bd9cd88-fe45-4216-938b-f97437e15450'

  // Service Bus
  ServiceBusDataOwner: '090c5cfd-751d-490a-894a-3ce6f1109419'
  ServiceBusDataSender: '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
  ServiceBusDataReceiver: '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'

  // Event Grid
  EventGridContributor: '1e241071-0855-49ea-94dc-649edcd759de'
  EventGridDataSender: 'd5a91429-5739-47e2-a06b-3470a27159e7'

  // Storage
  StorageBlobDataContributor: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
  StorageBlobDataReader: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
  StorageQueueDataContributor: '974c5e8b-45b9-4653-ba55-5f855dd0fb88'

  // App Configuration
  AppConfigurationDataReader: '516239f1-63e1-4d78-a4de-a74fb236a071'
  AppConfigurationDataOwner: '5ae67dd6-50cb-40e7-96ff-dc2bfa4b606b'

  // Monitoring
  MonitoringContributor: '749f88d5-cbae-40b8-bcfc-e573ddc772fa'
  MonitoringReader: '43d0d8ad-25c7-4714-9337-8ba259a9fe05'
}

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

// Check if roleDefinitionId is a built-in role name or a GUID
var isBuiltInRoleName = contains(builtInRoles, roleDefinitionId)
var resolvedRoleDefinitionId = isBuiltInRoleName
  ? builtInRoles[roleDefinitionId]
  : roleDefinitionId

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, resolvedRoleDefinitionId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', resolvedRoleDefinitionId)
    principalId: principalId
    principalType: principalType
    description: !empty(description) ? description : null
  }
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Role assignment ID')
output id string = roleAssignment.id

@description('Role assignment name')
output name string = roleAssignment.name

@description('Resolved role definition ID')
output roleDefinitionId string = resolvedRoleDefinitionId
