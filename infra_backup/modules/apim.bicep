// -----------------------------------------------------------------------------
// Existing API Management Integration Module
// -----------------------------------------------------------------------------
// This module attaches MCP API + operations + product to an already provisioned
// APIM instance. Expectations / prerequisites:
// 1. The APIM instance is deployed in External (public) or None network mode so
//    that the public gateway remains reachable. If you require private access
//    only, adapt policies & remove public exposure separately.
// 2. For shared VNet scenarios: If the APIM instance is integrated with a VNet
//    (External mode + subnet), it should be the SAME VNet used by the Function
//    App (or a peered VNet) to ensure private dependencies resolve consistently.
// 3. This template does not modify APIM networking configuration; that remains
//    centrally managed. Only logical API + product resources are added.
// 4. Managed identity authentication to backend Function relies on existing
//    identity assignments (configure separately if needed).
// -----------------------------------------------------------------------------
@description('API Management service name (existing)')
param apimName string

@description('Function App name to proxy to')
param functionAppName string

@description('API name for the MCP endpoints')
param apiName string = 'BestBot-mcp-api'

@description('API path for the MCP endpoints')
param apiPath string = 'bestbot-mcp'

@description('Product name for subscription management')
param productName string = 'BestBot-mcp-product'

// Reference existing API Management service
resource apim 'Microsoft.ApiManagement/service@2023-05-01-preview' existing = {
  name: apimName  
}

// API for the Function App
resource mcpApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apim
  name: apiName
  properties: {
    displayName: 'BestBot MCP API'
    description: 'Model Context Protocol endpoints for BestBot'
    path: apiPath
    protocols: ['https']
    serviceUrl: 'https://${functionAppName}.azurewebsites.net'
    subscriptionRequired: true
    apiRevision: '1'
    apiVersion: 'v1'
    apiVersionSetId: mcpVersionSet.id
  }
}

// API Version Set
resource mcpVersionSet 'Microsoft.ApiManagement/service/apiVersionSets@2023-05-01-preview' = {
  parent: apim
  name: 'BestBot-mcp-version-set'
  properties: {
    displayName: 'BestBot MCP API Versions'
    versioningScheme: 'Segment'
    versionHeaderName: 'Api-Version'
  }
}

// MCP runtime operations
resource mcpRuntimeOperation 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: mcpApi
  name: 'mcp-runtime'
  properties: {
    displayName: 'MCP Runtime SSE Endpoint'
    method: 'GET'
    urlTemplate: '/runtime/webhooks/mcp/sse'
    description: 'Server-Sent Events endpoint for MCP runtime communication'
    responses: [
      {
        statusCode: 200
        description: 'Successful response'
        headers: [
          {
            name: 'Content-Type'
            description: 'Content type header'
            type: 'string'
            values: ['text/event-stream']
          }
        ]
      }
    ]
  }
}

// POST operation for MCP tools
resource mcpToolsOperation 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: mcpApi
  name: 'mcp-tools'
  properties: {
    displayName: 'MCP Tools Endpoint'
    method: 'POST'
    urlTemplate: '/runtime/webhooks/mcp/*'
    description: 'POST endpoint for MCP tool invocations'
    responses: [
      {
        statusCode: 200
        description: 'Successful response'
      }
    ]
  }
}

// Product for subscription management
resource mcpProduct 'Microsoft.ApiManagement/service/products@2023-05-01-preview' = {
  parent: apim
  name: productName
  properties: {
    displayName: 'BestBot MCP Product'
    description: 'Product for accessing BestBot MCP APIs'
    subscriptionRequired: true
    approvalRequired: false
    state: 'published'
  }
}

// Associate API with Product
resource productApi 'Microsoft.ApiManagement/service/products/apis@2023-05-01-preview' = {
  parent: mcpProduct
  name: mcpApi.name
}

// Policy for the API to require subscription key and forward to Function App
resource mcpApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  parent: mcpApi
  name: 'policy'
  properties: {
    value: format('''
<policies>
  <inbound>
    <base />
    <set-backend-service base-url="https://{0}.azurewebsites.net" />
    <authentication-managed-identity resource="https://{0}.azurewebsites.net" />
    <set-header name="X-Forwarded-Host" exists-action="override">
      <value>@(context.Request.OriginalUrl.Host)</value>
    </set-header>
    <set-header name="X-Forwarded-Proto" exists-action="override">
      <value>https</value>
    </set-header>
    <set-header name="X-Original-URL" exists-action="override">
      <value>@(context.Request.OriginalUrl.ToString())</value>
    </set-header>
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
''', functionAppName)
  }
}

// Outputs
output apimName string = apim.name
output apimId string = apim.id
output apimGatewayUrl string = 'https://${apim.properties.gatewayUrl}'
output apimManagementUrl string = 'https://${apim.properties.managementApiUrl}'
output apimDeveloperPortalUrl string = 'https://${apim.properties.developerPortalUrl}'
output mcpApiPath string = '/${apiPath}'
output mcpEndpoint string = 'https://${apim.properties.gatewayUrl}/${apiPath}'
output apiName string = mcpApi.name
output productName string = mcpProduct.name
