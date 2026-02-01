// =============================================================================
// Managed Identity Module
// Provides user-assigned managed identity for passwordless authentication
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Name of the managed identity')
param identityName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
  tags: tags
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Managed Identity resource ID')
output id string = managedIdentity.id

@description('Managed Identity name')
output name string = managedIdentity.name

@description('Managed Identity principal ID (object ID)')
output principalId string = managedIdentity.properties.principalId

@description('Managed Identity client ID')
output clientId string = managedIdentity.properties.clientId

@description('Managed Identity tenant ID')
output tenantId string = managedIdentity.properties.tenantId

@description('Managed Identity resource for reference')
output resource object = {
  id: managedIdentity.id
  name: managedIdentity.name
  principalId: managedIdentity.properties.principalId
  clientId: managedIdentity.properties.clientId
}
