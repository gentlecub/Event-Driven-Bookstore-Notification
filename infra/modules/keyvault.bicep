// =============================================================================
// Key Vault Module
// Provides secure secrets management following zero-trust principles
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Name of the Key Vault')
param keyVaultName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

@description('Key Vault SKU')
@allowed(['standard', 'premium'])
param sku string = 'standard'

@description('Enable soft delete')
param enableSoftDelete bool = true

@description('Soft delete retention in days (7-90)')
@minValue(7)
@maxValue(90)
param softDeleteRetentionDays int = 90

@description('Enable purge protection (cannot be disabled once enabled)')
param enablePurgeProtection bool = false

@description('Enable RBAC authorization (recommended over access policies)')
param enableRbacAuthorization bool = true

@description('Principal IDs to grant Key Vault Secrets User role')
param secretsUserPrincipalIds array = []

@description('Principal IDs to grant Key Vault Secrets Officer role')
param secretsOfficerPrincipalIds array = []

@description('Enable public network access')
param enablePublicNetworkAccess bool = true

@description('Allowed IP ranges for network rules')
param allowedIpRanges array = []

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var networkAcls = enablePublicNetworkAccess ? {
  defaultAction: 'Allow'
  bypass: 'AzureServices'
  ipRules: [for ip in allowedIpRanges: {
    value: ip
  }]
  virtualNetworkRules: []
} : {
  defaultAction: 'Deny'
  bypass: 'AzureServices'
  ipRules: []
  virtualNetworkRules: []
}

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: sku
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionDays
    enablePurgeProtection: enablePurgeProtection ? true : null
    enableRbacAuthorization: enableRbacAuthorization
    publicNetworkAccess: enablePublicNetworkAccess ? 'Enabled' : 'Disabled'
    networkAcls: networkAcls
  }
}

// -----------------------------------------------------------------------------
// RBAC Role Assignments
// -----------------------------------------------------------------------------

// Key Vault Secrets User - Read secrets
@description('Key Vault Secrets User role definition ID')
var secretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource secretsUserRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in secretsUserPrincipalIds: {
  name: guid(keyVault.id, principalId, secretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', secretsUserRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]

// Key Vault Secrets Officer - Manage secrets
@description('Key Vault Secrets Officer role definition ID')
var secretsOfficerRoleId = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

resource secretsOfficerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = [for principalId in secretsOfficerPrincipalIds: {
  name: guid(keyVault.id, principalId, secretsOfficerRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', secretsOfficerRoleId)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}]

// -----------------------------------------------------------------------------
// Diagnostic Settings (placeholder for monitoring module)
// -----------------------------------------------------------------------------

// Will be added in Module 12 - Monitoring

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Key Vault resource ID')
output id string = keyVault.id

@description('Key Vault name')
output name string = keyVault.name

@description('Key Vault URI')
output vaultUri string = keyVault.properties.vaultUri

@description('Key Vault resource for reference')
output resource object = {
  id: keyVault.id
  name: keyVault.name
  uri: keyVault.properties.vaultUri
}
