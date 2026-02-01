// =============================================================================
// Bicep User-Defined Types
// Provides strongly-typed parameter definitions for all deployments
// =============================================================================

// -----------------------------------------------------------------------------
// Environment Configuration Type
// -----------------------------------------------------------------------------

@description('Valid environment names')
@export()
type environmentType = 'dev' | 'test' | 'stg' | 'prod'

// -----------------------------------------------------------------------------
// Azure Region Configuration
// -----------------------------------------------------------------------------

@description('Supported Azure regions')
@export()
type azureRegionType = 'eastus' | 'eastus2' | 'westus' | 'westus2' | 'centralus' | 'northeurope' | 'westeurope'

// -----------------------------------------------------------------------------
// Cosmos DB Configuration
// -----------------------------------------------------------------------------

@description('Cosmos DB SKU/throughput configuration')
@export()
type cosmosDbConfigType = {
  @description('Enable serverless mode (cost-effective for dev)')
  serverless: bool

  @description('Provisioned throughput in RU/s (ignored if serverless)')
  throughput: int

  @description('Enable automatic failover')
  enableAutomaticFailover: bool

  @description('Enable multi-region writes')
  enableMultipleWriteLocations: bool

  @description('Backup policy type')
  backupPolicy: 'Continuous' | 'Periodic'
}

// -----------------------------------------------------------------------------
// Service Bus Configuration
// -----------------------------------------------------------------------------

@description('Service Bus SKU options')
@export()
type serviceBusSkuType = 'Basic' | 'Standard' | 'Premium'

@description('Service Bus configuration')
@export()
type serviceBusConfigType = {
  @description('Service Bus SKU tier')
  sku: serviceBusSkuType

  @description('Capacity units (Premium only, 1-16)')
  capacity: int

  @description('Enable zone redundancy (Premium only)')
  zoneRedundant: bool

  @description('Message retention in days')
  messageRetentionDays: int
}

// -----------------------------------------------------------------------------
// API Management Configuration
// -----------------------------------------------------------------------------

@description('APIM SKU options')
@export()
type apimSkuType = 'Consumption' | 'Developer' | 'Basic' | 'Standard' | 'Premium'

@description('API Management configuration')
@export()
type apimConfigType = {
  @description('APIM SKU tier')
  sku: apimSkuType

  @description('Number of scale units')
  capacity: int

  @description('Publisher email for notifications')
  publisherEmail: string

  @description('Publisher organization name')
  publisherName: string
}

// -----------------------------------------------------------------------------
// Function App Configuration
// -----------------------------------------------------------------------------

@description('Function App hosting plan type')
@export()
type functionPlanType = 'Consumption' | 'Premium' | 'Dedicated'

@description('Function App configuration')
@export()
type functionAppConfigType = {
  @description('Hosting plan type')
  planType: functionPlanType

  @description('App Service Plan SKU (for Premium/Dedicated)')
  planSku: string

  @description('Always On setting (not available in Consumption)')
  alwaysOn: bool

  @description('Minimum instance count (Premium only)')
  minimumInstances: int

  @description('Maximum instance count')
  maximumInstances: int
}

// -----------------------------------------------------------------------------
// Key Vault Configuration
// -----------------------------------------------------------------------------

@description('Key Vault SKU options')
@export()
type keyVaultSkuType = 'standard' | 'premium'

@description('Key Vault configuration')
@export()
type keyVaultConfigType = {
  @description('Key Vault SKU')
  sku: keyVaultSkuType

  @description('Enable soft delete')
  enableSoftDelete: bool

  @description('Soft delete retention days')
  softDeleteRetentionDays: int

  @description('Enable purge protection')
  enablePurgeProtection: bool
}

// -----------------------------------------------------------------------------
// Monitoring Configuration
// -----------------------------------------------------------------------------

@description('Log Analytics pricing tier')
@export()
type logAnalyticsTierType = 'Free' | 'PerGB2018' | 'Standalone'

@description('Monitoring configuration')
@export()
type monitoringConfigType = {
  @description('Log Analytics pricing tier')
  logAnalyticsTier: logAnalyticsTierType

  @description('Data retention in days')
  retentionDays: int

  @description('Enable Application Insights')
  enableAppInsights: bool

  @description('App Insights daily cap in GB')
  dailyCapGb: int
}

// -----------------------------------------------------------------------------
// Complete Environment Configuration
// -----------------------------------------------------------------------------

@description('Complete environment configuration object')
@export()
type environmentConfigType = {
  @description('Environment name')
  environment: environmentType

  @description('Azure region')
  location: azureRegionType

  @description('Cosmos DB settings')
  cosmosDb: cosmosDbConfigType

  @description('Service Bus settings')
  serviceBus: serviceBusConfigType

  @description('API Management settings')
  apim: apimConfigType

  @description('Function App settings')
  functionApp: functionAppConfigType

  @description('Key Vault settings')
  keyVault: keyVaultConfigType

  @description('Monitoring settings')
  monitoring: monitoringConfigType
}
