# Infrastructure Templates

This folder contains Azure Bicep templates for deploying BestBot to Azure.

## Files

- `main.bicep` - Main infrastructure template that deploys the complete BestBot solution
- `main.parameters.json` - Default parameters file for local deployment
- `modules/apim.bicep` - Azure API Management module for fronting the Function App

## Deployment Options

### Basic Deployment (Function App only)

Deploy just the Function App without API Management:

```bash
azd up
```

This creates:
- Azure Function App (Flex Consumption plan)
- Storage Account for Function App state
- Application Insights for monitoring
- Log Analytics workspace
- User-assigned managed identity

### APIM Integration Deployment

Deploy with Azure API Management fronting the Function App:

```bash
# Configure APIM parameters
azd env set deployApim true
azd env set apimPublisherEmail "your-email@example.com"
azd env set apimSku "Developer"

# Deploy
azd up
```

This adds:
- Azure API Management service
- API configuration for MCP endpoints
- Function App IP restrictions (APIM-only access)
- Managed identity authentication between APIM and Function App

## Parameters

### Main Template Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `location` | string | "southcentralus" | Azure region for deployment |
| `environmentName` | string | *required* | Environment name for resource naming |
| `deployApim` | bool | false | Whether to deploy API Management |
| `apimSku` | string | "Developer" | APIM SKU (Consumption, Developer, Standard, Premium) |
| `apimPublisherEmail` | string | "" | Publisher email for APIM (required if deployApim is true) |
| `apimPublisherName` | string | "BestBot Team" | Publisher name for APIM |
| `enableApimVnet` | bool | false | Whether to enable VNet integration for APIM |

### APIM Module Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `location` | string | Azure region to deploy to |
| `tags` | object | Tags to apply to resources |
| `apimName` | string | API Management service name |
| `apimSku` | string | API Management SKU |
| `publisherEmail` | string | API Management publisher email |
| `publisherName` | string | API Management publisher name |
| `functionAppName` | string | Function App name to proxy to |
| `managedIdentityId` | string | Managed identity resource ID for APIM |
| `enableVnet` | bool | Whether to enable VNet integration |
| `subnetId` | string | Subnet resource ID for VNet integration |

## Outputs

### Main Template Outputs

| Output | Type | Description |
|--------|------|-------------|
| `functionAppName` | string | Name of the deployed Function App |
| `functionAppResourceId` | string | Resource ID of the Function App |
| `applicationInsightsConnectionString` | string | Application Insights connection string |
| `storageAccountName` | string | Name of the storage account |
| `apimDeployed` | bool | Whether APIM was deployed |
| `apimName` | string | Name of the APIM service (if deployed) |
| `apimGatewayUrl` | string | APIM gateway URL (if deployed) |
| `apimManagementUrl` | string | APIM management URL (if deployed) |
| `apimDeveloperPortalUrl` | string | APIM developer portal URL (if deployed) |
| `mcpEndpoint` | string | Primary MCP endpoint URL |

## Security Configuration

### Without APIM

- Function App accepts traffic from all sources
- CORS enabled for all origins
- HTTPS enforced

### With APIM

- Function App restricted to accept traffic only from Azure API Management service
- APIM uses managed identity for authentication with Function App
- Subscription keys required for API access
- HTTPS enforced throughout

## Cost Considerations

### Function App (Always Deployed)

- **Flex Consumption Plan**: Pay-per-execution model
- **Storage Account**: Standard LRS for minimal cost
- **Application Insights**: Pay-per-GB ingested

### API Management (Optional)

- **Developer SKU**: ~$50/month, suitable for dev/test
- **Consumption SKU**: Pay-per-call (if available in region)
- **Standard/Premium**: Higher cost, production scenarios

## Troubleshooting

### APIM Deployment Issues

1. **Publisher email required**: Ensure `apimPublisherEmail` is set when `deployApim` is true
2. **Long deployment times**: APIM can take 30-45 minutes to deploy
3. **SKU availability**: Consumption tier may not be available in all regions

### Function App Access Issues

1. **403 Forbidden**: When APIM is enabled, direct Function App access is blocked
2. **Subscription key required**: APIM endpoints require valid subscription keys
3. **IP restrictions**: Check that APIM service tag is properly configured

### Rollback Procedures

To remove APIM and restore direct Function App access:

```bash
azd env set deployApim false
azd up
```

This will:
- Remove the APIM service
- Remove IP restrictions from Function App
- Restore direct public access to Function App endpoints
