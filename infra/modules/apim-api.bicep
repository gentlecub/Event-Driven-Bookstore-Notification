// =============================================================================
// API Management API Definition Module
// Creates an API with operations, policies, and product associations
// =============================================================================

// -----------------------------------------------------------------------------
// Parameters
// -----------------------------------------------------------------------------

@description('Name of the parent APIM service')
param apimName string

@description('API identifier (used in URL path)')
param apiId string

@description('API display name')
param apiDisplayName string

@description('API description')
param apiDescription string = ''

@description('API path prefix')
param apiPath string

@description('API version (e.g., v1)')
param apiVersion string = 'v1'

@description('Backend service URL')
param backendUrl string

@description('OpenAPI specification content')
param openApiSpec string = ''

@description('Product IDs to associate with this API')
param productIds array = ['starter', 'unlimited']

@description('Enable subscription key requirement')
param subscriptionRequired bool = true

// -----------------------------------------------------------------------------
// Variables
// -----------------------------------------------------------------------------

var apiName = '${apiId}-${apiVersion}'

// -----------------------------------------------------------------------------
// Resources
// -----------------------------------------------------------------------------

// Reference to existing APIM
resource apim 'Microsoft.ApiManagement/service@2023-05-01-preview' existing = {
  name: apimName
}

// API Definition
resource api 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apim
  name: apiName
  properties: {
    displayName: '${apiDisplayName} ${apiVersion}'
    description: apiDescription
    path: apiPath
    protocols: ['https']
    subscriptionRequired: subscriptionRequired
    subscriptionKeyParameterNames: {
      header: 'Ocp-Apim-Subscription-Key'
      query: 'subscription-key'
    }
    apiVersion: apiVersion
    apiVersionSetId: apiVersionSet.id
    serviceUrl: backendUrl
    format: !empty(openApiSpec) ? 'openapi+json' : null
    value: !empty(openApiSpec) ? openApiSpec : null
  }
}

// API Version Set
resource apiVersionSet 'Microsoft.ApiManagement/service/apiVersionSets@2023-05-01-preview' = {
  parent: apim
  name: apiId
  properties: {
    displayName: apiDisplayName
    versioningScheme: 'Segment'
  }
}

// API Policy
resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  parent: api
  name: 'policy'
  properties: {
    format: 'xml'
    value: '''
<policies>
  <inbound>
    <base />
    <!-- Set backend URL -->
    <set-backend-service base-url="{{function-app-url}}" />
    <!-- Add correlation ID -->
    <set-header name="X-Correlation-ID" exists-action="skip">
      <value>@(context.RequestId.ToString())</value>
    </set-header>
  </inbound>
  <backend>
    <base />
  </backend>
  <outbound>
    <base />
    <!-- Remove internal headers -->
    <set-header name="X-Powered-By" exists-action="delete" />
    <set-header name="X-AspNet-Version" exists-action="delete" />
  </outbound>
  <on-error>
    <base />
    <!-- Custom error response -->
    <return-response>
      <set-status code="500" reason="Internal Server Error" />
      <set-header name="Content-Type" exists-action="override">
        <value>application/json</value>
      </set-header>
      <set-body>@{
        return new JObject(
          new JProperty("error", new JObject(
            new JProperty("code", context.Response.StatusCode),
            new JProperty("message", "An error occurred processing your request"),
            new JProperty("correlationId", context.RequestId.ToString())
          ))
        ).ToString();
      }</set-body>
    </return-response>
  </on-error>
</policies>
'''
  }
}

// Product Associations
resource productAssociations 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = [for productId in productIds: {
  name: '${apimName}/${productId}/${apiName}'
}]

// -----------------------------------------------------------------------------
// Outputs
// -----------------------------------------------------------------------------

@description('API resource ID')
output id string = api.id

@description('API name')
output name string = api.name

@description('API path')
output path string = api.properties.path
