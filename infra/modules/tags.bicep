// =============================================================================
// Tagging Module
// Generates consistent resource tags following CAF tagging standards
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Environment name (dev, test, stg, prod)')
@allowed(['dev', 'test', 'stg', 'prod'])
param environment string

@description('Owner of the resources (team or person)')
param owner string = 'devteam'

@description('Cost center for billing allocation')
param costCenter string = ''

@description('Additional custom tags to merge')
param customTags object = {}

// -----------------------------------------------------------------------------
// Variables - Base Tags (Required)
// -----------------------------------------------------------------------------

var baseTags = {
  Environment: environment
  Project: 'bookstore'
  Owner: owner
  ManagedBy: 'bicep'
}

// -----------------------------------------------------------------------------
// Variables - Environment-Specific Tags
// -----------------------------------------------------------------------------

var environmentTags = {
  dev: {
    Criticality: 'low'
  }
  test: {
    Criticality: 'medium'
  }
  stg: {
    Criticality: 'medium'
  }
  prod: {
    Criticality: 'high'
    DataClassification: 'internal'
  }
}

// -----------------------------------------------------------------------------
// Variables - Optional Tags
// -----------------------------------------------------------------------------

var optionalTags = !empty(costCenter)
  ? {
      CostCenter: costCenter
    }
  : {}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('Complete set of tags for resources')
output tags object = union(baseTags, environmentTags[environment], optionalTags, customTags)

@description('Base tags only (required tags)')
output baseTags object = baseTags

@description('Environment-specific tags')
output environmentTags object = environmentTags[environment]
