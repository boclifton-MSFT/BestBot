# Azure API Management Module Options

This directory contains multiple Bicep modules for deploying Azure API Management resources depending on your scenario.

## Module Options

### 1. `apim.bicep` - Use Existing APIM Service (Recommended for existing deployments)

**Use this when**: You have an existing Azure API Management service and want to add BestBot MCP APIs to it.

**What it does**: 
- References an existing APIM service using the `existing` keyword
- Creates only the API, operations, product, and policies
- Does not modify the existing APIM service configuration

**Required Parameters**:
```bicep
param apimName string              // Name of your existing APIM service
param functionAppName string       // Your Azure Function App name
```

**Optional Parameters**:
```bicep
param apiName string = 'bestbot-mcp-api'           // Custom API name
param apiPath string = 'mcp'                       // Custom API path
param productName string = 'bestbot-mcp-product'   // Custom product name
```

**Usage in main template**:
```bicep
module apimModule 'modules/apim.bicep' = {
  name: 'apim-deployment'
  params: {
    apimName: 'my-existing-apim-service'
    functionAppName: functionApp.outputs.name
  }
}
```

### 2. `apim-new.bicep` - Create New APIM Service

**Use this when**: You need to create a brand new Azure API Management service with BestBot MCP APIs.

**What it does**:
- Creates a new APIM service from scratch
- Configures security settings, managed identity, and VNet integration
- Creates API, operations, product, and policies

**Required Parameters**:
```bicep
param location string
param apimName string
param publisherEmail string
param functionAppName string
param managedIdentityId string
```

**Usage in main template**:
```bicep
module apimModule 'modules/apim-new.bicep' = {
  name: 'apim-deployment'
  params: {
    location: location
    apimName: 'my-new-apim-service'
    publisherEmail: 'admin@company.com'
    functionAppName: functionApp.outputs.name
    managedIdentityId: managedIdentity.id
    apimSku: 'Developer'
  }
}
```

### 3. `apim-existing.bicep` - Simplified Existing APIM

**Use this when**: You have an existing APIM service in the same resource group and want minimal configuration.

**What it does**:
- Simple reference to existing APIM
- Creates API resources with minimal parameters
- Good for quick testing or simple scenarios

## Cross-Resource Group Deployment

If your existing APIM service is in a different resource group, you have two options:

### Option A: Use Module with Target Scope
Create a wrapper module that targets the correct resource group:

```bicep
// apim-cross-rg.bicep
targetScope = 'resourceGroup'

param apimResourceGroup string
param apimName string
param functionAppName string

module apimModule 'apim.bicep' = {
  name: 'apim-deployment'
  scope: resourceGroup(apimResourceGroup)
  params: {
    apimName: apimName
    functionAppName: functionAppName
  }
}
```

### Option B: Use Azure CLI/PowerShell
Deploy the module directly to the target resource group:

```bash
# Azure CLI
az deployment group create \
  --resource-group "apim-resource-group" \
  --template-file "modules/apim.bicep" \
  --parameters apimName="my-apim" functionAppName="my-function-app"
```

```powershell
# PowerShell
New-AzResourceGroupDeployment `
  -ResourceGroupName "apim-resource-group" `
  -TemplateFile "modules/apim.bicep" `
  -apimName "my-apim" `
  -functionAppName "my-function-app"
```

## Security Considerations

1. **Managed Identity**: When creating new APIM, ensure you provide a user-assigned managed identity
2. **Subscription Keys**: The API requires subscription keys for access
3. **HTTPS Only**: All endpoints are configured for HTTPS only
4. **TLS Security**: Older TLS versions are disabled for security

## Outputs

All modules provide these outputs:
- `apimName`: Name of the APIM service
- `apimId`: Resource ID of the APIM service  
- `apimGatewayUrl`: Gateway URL for API access
- `mcpEndpoint`: Full URL to the MCP API endpoint
- `apiName`: Name of the created API
- `productName`: Name of the created product

## Best Practices

1. **Use existing APIM**: If you already have an APIM service, use `apim.bicep` to avoid creating duplicate services
2. **Resource naming**: Use consistent naming conventions for APIs and products
3. **API versioning**: The modules include API version sets for future versioning
4. **Monitoring**: Existing APIM services should already have Application Insights configured
5. **Access control**: Review and configure appropriate subscription and access policies

## Troubleshooting

- **Scope errors**: Ensure the APIM service exists in the target resource group
- **Permission errors**: Verify you have Contributor access to the APIM service
- **Naming conflicts**: Check for existing API or product names that might conflict
- **SKU limitations**: Some features require specific APIM SKU tiers
