# Using Existing Azure API Management Service

To modify your deployment to use an existing Azure API Management service instead of creating a new one, you have several options:

## Option 1: Update Existing main.bicep Template

Replace the APIM module section in your `main.bicep`:

```bicep
// Replace the existing parameters with these for existing APIM
@description('Whether to use an existing API Management service')
param useExistingApim bool = false

@description('Name of existing APIM service (required if useExistingApim is true)')
param existingApimName string = ''

@description('Resource group of existing APIM service (leave empty if same as deployment)')
param existingApimResourceGroup string = ''

// Replace the existing APIM module deployment with this:
module apimModule 'modules/apim.bicep' = if (useExistingApim) {
  name: 'apim-deployment'
  scope: !empty(existingApimResourceGroup) ? resourceGroup(existingApimResourceGroup) : resourceGroup()
  params: {
    apimName: existingApimName
    functionAppName: site.name
    // Optional customizations
    apiName: 'bestbot-mcp-api'
    apiPath: 'mcp'
    productName: 'bestbot-mcp-product'
  }
}

// Update the outputs section
output apimDeployed bool = useExistingApim
output apimName string = useExistingApim && apimModule != null ? apimModule.outputs.apimName : ''
output apimGatewayUrl string = useExistingApim && apimModule != null ? apimModule.outputs.apimGatewayUrl : ''
output apimManagementUrl string = useExistingApim && apimModule != null ? apimModule.outputs.apimManagementUrl : ''
output apimDeveloperPortalUrl string = useExistingApim && apimModule != null ? apimModule.outputs.apimDeveloperPortalUrl : ''
output mcpEndpoint string = useExistingApim && apimModule != null ? apimModule.outputs.mcpEndpoint : 'https://${site.name}.azurewebsites.net'
```

## Option 2: Use Dedicated Deployment

Create a separate deployment specifically for adding BestBot to existing APIM:

### Step 1: Create dedicated deployment file

Create `infra/deploy-to-existing-apim.bicep`:

```bicep
targetScope = 'resourceGroup'

@description('Name of existing APIM service')
param existingApimName string

@description('Function App name to connect to')
param functionAppName string

@description('API name for the MCP endpoints')
param apiName string = 'bestbot-mcp-api'

@description('API path for the MCP endpoints')
param apiPath string = 'mcp'

@description('Product name for subscription management')
param productName string = 'bestbot-mcp-product'

module apimModule 'modules/apim.bicep' = {
  name: 'apim-deployment'
  params: {
    apimName: existingApimName
    functionAppName: functionAppName
    apiName: apiName
    apiPath: apiPath
    productName: productName
  }
}

output apimName string = apimModule.outputs.apimName
output mcpEndpoint string = apimModule.outputs.mcpEndpoint
output apiName string = apimModule.outputs.apiName
output productName string = apimModule.outputs.productName
```

### Step 2: Create parameters file

Create `infra/deploy-to-existing-apim.parameters.json`:

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "existingApimName": {
      "value": "your-existing-apim-service-name"
    },
    "functionAppName": {
      "value": "your-function-app-name"
    },
    "apiPath": {
      "value": "bestbot"
    }
  }
}
```

### Step 3: Deploy

```bash
# Azure CLI
az deployment group create \
  --resource-group "your-apim-resource-group" \
  --template-file "infra/deploy-to-existing-apim.bicep" \
  --parameters @infra/deploy-to-existing-apim.parameters.json

# Or with inline parameters
az deployment group create \
  --resource-group "your-apim-resource-group" \
  --template-file "infra/deploy-to-existing-apim.bicep" \
  --parameters existingApimName="my-apim" functionAppName="my-function-app"
```

```powershell
# PowerShell
New-AzResourceGroupDeployment `
  -ResourceGroupName "your-apim-resource-group" `
  -TemplateFile "infra/deploy-to-existing-apim.bicep" `
  -TemplateParameterFile "infra/deploy-to-existing-apim.parameters.json"
```

## Option 3: Azure Developer CLI (azd) Integration

Update your `azure.yaml` to support existing APIM:

```yaml
# azure.yaml
name: bestbot
metadata:
  template: bestbot@latest
services:
  function:
    project: .
    language: dotnet
    host: azurefunctions

infra:
  provider: bicep
  
hooks:
  preup:
    shell: pwsh
    run: |
      # Check if user wants to use existing APIM
      $useExisting = Read-Host "Use existing APIM service? (y/n)"
      if ($useExisting -eq "y") {
        $apimName = Read-Host "Enter existing APIM service name"
        $apimRg = Read-Host "Enter APIM resource group (press Enter if same as deployment)"
        azd env set USE_EXISTING_APIM true
        azd env set EXISTING_APIM_NAME $apimName
        if (![string]::IsNullOrEmpty($apimRg)) {
          azd env set EXISTING_APIM_RG $apimRg
        }
      }
```

Then update your main.parameters.json to read from environment:

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "environmentName": {
      "value": "${AZURE_ENV_NAME}"
    },
    "location": {
      "value": "${AZURE_LOCATION}"
    },
    "useExistingApim": {
      "value": "${USE_EXISTING_APIM:false}"
    },
    "existingApimName": {
      "value": "${EXISTING_APIM_NAME:}"
    },
    "existingApimResourceGroup": {
      "value": "${EXISTING_APIM_RG:}"
    }
  }
}
```

## Security Considerations

When using existing APIM:

1. **Access Control**: Ensure your deployment has Contributor access to the APIM service
2. **Subscription Management**: The module creates a new product that requires subscription keys
3. **IP Restrictions**: Remove Function App IP restrictions or configure them to allow APIM traffic
4. **Managed Identity**: Ensure APIM has proper permissions to access your Function App

## Validation

After deployment, verify:

1. **API Created**: Check Azure portal that the BestBot MCP API was created
2. **Operations**: Verify the MCP runtime and tools operations exist
3. **Product**: Confirm the product is published and available
4. **Subscription**: Test API access with a subscription key
5. **Endpoint**: Test the MCP endpoint: `https://your-apim-gateway/mcp/runtime/webhooks/mcp/sse`