@description('Azure region to deploy to')
param location string

@description('Tags to apply to resources')
param tags object = {}

@description('API Management service name')
param apimName string

@description('API Management SKU')
@allowed(['Basic', 'BasicV2', 'Consumption', 'Developer', 'Isolated', 'Premium', 'Standard', 'StandardV2'])
param apimSku string = 'Developer'

@description('API Management publisher email')
param publisherEmail string

@description('API Management publisher name')
param publisherName string = 'BestBot Team'

@description('Function App name to proxy to')
param functionAppName string

@description('Managed identity resource ID for APIM')
param managedIdentityId string

@description('Whether to enable VNet integration (optional)')
param enableVnet bool = false

@description('Subnet resource ID for VNet integration (required if enableVnet is true)')
param subnetId string = ''

@description('API name for the MCP endpoints')
param apiName string = 'bestbot-mcp-api'

@description('API path for the MCP endpoints')
param apiPath string = 'mcp'

@description('Product name for subscription management')
param productName string = 'bestbot-mcp-product'

// API Management service (creates new)
resource apim 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apimName
  location: location
  tags: tags
  sku: {
    name: apimSku
    capacity: apimSku == 'Consumption' ? 0 : 1
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
    virtualNetworkType: enableVnet ? 'Internal' : 'None'
    virtualNetworkConfiguration: enableVnet ? {
      subnetResourceId: subnetId
    } : null
    apiVersionConstraint: {
      minApiVersion: '2019-12-01'
    }
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Ssl30': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'False'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'False'
    }
  }
}

// API Version Set
resource mcpVersionSet 'Microsoft.ApiManagement/service/apiVersionSets@2023-05-01-preview' = {
  parent: apim
  name: 'bestbot-mcp-version-set'
  properties: {
    displayName: 'BestBot MCP API Versions'
    versioningScheme: 'Segment'
    versionHeaderName: 'Api-Version'
  }
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
