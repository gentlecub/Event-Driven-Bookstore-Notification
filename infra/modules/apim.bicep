// =============================================================================
// API Management Module
// Provides API gateway for the Bookstore Notification System
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('API Management service name')
param apimName string

@description('Azure region for deployment')
param location string

@description('Tags to apply to resources')
param tags object

@description('APIM SKU')
@allowed(['Consumption', 'Developer', 'Basic', 'Standard', 'Premium'])
param sku string = 'Consumption'

@description('APIM capacity (ignored for Consumption)')
param capacity int = 0

@description('Publisher email address')
param publisherEmail string

@description('Publisher organization name')
param publisherName string

@description('Application Insights resource ID for logging')
param appInsightsId string = ''

@description('Application Insights instrumentation key')
param appInsightsKey string = ''

@description('Function App base URL for backend')
param functionAppBaseUrl string = ''

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var isConsumptionSku = sku == 'Consumption'

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// API Management Service
resource apim 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apimName
  location: location
  tags: tags
  sku: {
    name: sku
    capacity: isConsumptionSku ? 0 : capacity
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
    customProperties: isConsumptionSku ? {} : {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'false'
    }
  }
}

// -----------------------------------------------------------------------------
// Named Values (Configuration)
// -----------------------------------------------------------------------------

resource namedValueFunctionUrl 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = if (!empty(functionAppBaseUrl)) {
  parent: apim
  name: 'function-app-url'
  properties: {
    displayName: 'function-app-url'
    value: functionAppBaseUrl
    secret: false
  }
}

// -----------------------------------------------------------------------------
// Application Insights Logger
// -----------------------------------------------------------------------------

resource apimLogger 'Microsoft.ApiManagement/service/loggers@2023-05-01-preview' = if (!empty(appInsightsId)) {
  parent: apim
  name: 'app-insights-logger'
  properties: {
    loggerType: 'applicationInsights'
    resourceId: appInsightsId
    credentials: {
      instrumentationKey: appInsightsKey
    }
  }
}

// -----------------------------------------------------------------------------
// Products
// -----------------------------------------------------------------------------

// Starter Product (rate-limited, free tier)
resource starterProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apim
  name: 'starter'
  properties: {
    displayName: 'Starter'
    description: 'Free tier with rate limiting for evaluation'
    subscriptionRequired: true
    approvalRequired: false
    state: 'published'
    terms: 'Rate limited to 100 calls per minute'
  }
}

// Unlimited Product (for internal/premium use)
resource unlimitedProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apim
  name: 'unlimited'
  properties: {
    displayName: 'Unlimited'
    description: 'Unlimited access for premium subscribers'
    subscriptionRequired: true
    approvalRequired: true
    state: 'published'
  }
}

// -----------------------------------------------------------------------------
// Global Policies
// -----------------------------------------------------------------------------

resource globalPolicy 'Microsoft.ApiManagement/service/policies@2023-05-01-preview' = {
  parent: apim
  name: 'policy'
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
    <!-- CORS Policy -->
    <cors allow-credentials="false">
      <allowed-origins>
        <origin>*</origin>
      </allowed-origins>
      <allowed-methods>
        <method>GET</method>
        <method>POST</method>
        <method>PUT</method>
        <method>DELETE</method>
        <method>OPTIONS</method>
      </allowed-methods>
      <allowed-headers>
        <header>Content-Type</header>
        <header>Authorization</header>
        <header>Ocp-Apim-Subscription-Key</header>
      </allowed-headers>
    </cors>
    <base />
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <!-- Add response headers -->
    <set-header name="X-Content-Type-Options" exists-action="override">
      <value>nosniff</value>
    </set-header>
    <set-header name="X-Frame-Options" exists-action="override">
      <value>DENY</value>
    </set-header>
    <base />
  </outbound>
  <on-error>
    <base />
  </on-error>
</policies>
'''
  }
}

// -----------------------------------------------------------------------------
// Starter Product Policy (Rate Limiting)
// -----------------------------------------------------------------------------

resource starterProductPolicy 'Microsoft.ApiManagement/service/products/policies@2023-05-01-preview' = {
  parent: starterProduct
  name: 'policy'
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
    <!-- Rate limit: 100 calls per minute -->
    <rate-limit calls="100" renewal-period="60" />
    <!-- Quota: 1000 calls per day -->
    <quota calls="1000" renewal-period="86400" />
    <base />
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <base />
  </outbound>
  <on-error>
    <base />
  </on-error>
</policies>
'''
  }
}

// -----------------------------------------------------------------------------
// Diagnostic Settings
// -----------------------------------------------------------------------------

resource apimDiagnostics 'Microsoft.ApiManagement/service/diagnostics@2023-05-01-preview' = if (!empty(appInsightsId)) {
  parent: apim
  name: 'applicationinsights'
  properties: {
    alwaysLog: 'allErrors'
    loggerId: apimLogger.id
    sampling: {
      percentage: 100
      samplingType: 'fixed'
    }
    frontend: {
      request: {
        headers: ['X-Forwarded-For']
        body: { bytes: 1024 }
      }
      response: {
        headers: []
        body: { bytes: 1024 }
      }
    }
    backend: {
      request: {
        headers: []
        body: { bytes: 1024 }
      }
      response: {
        headers: []
        body: { bytes: 1024 }
      }
    }
  }
}

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('API Management resource ID')
output id string = apim.id

@description('API Management name')
output name string = apim.name

@description('API Management gateway URL')
output gatewayUrl string = apim.properties.gatewayUrl

@description('API Management developer portal URL')
output portalUrl string = isConsumptionSku ? '' : apim.properties.developerPortalUrl

@description('API Management system-assigned identity principal ID')
output identityPrincipalId string = apim.identity.principalId

@description('API Management resource')
output resource object = {
  id: apim.id
  name: apim.name
  gatewayUrl: apim.properties.gatewayUrl
  identityPrincipalId: apim.identity.principalId
}
