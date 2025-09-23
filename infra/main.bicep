@description('Azure region to deploy to')
param location string = 'southcentralus'

@description('Azd environment name')
param environmentName string

@description('Tags to apply to resources')
param tags object = {
  'azd-env-name': environmentName
}

@description('Whether to deploy API Management')
param deployApim bool = false

@description('API Management SKU')
@allowed(['Consumption', 'Developer', 'Standard', 'Premium'])
param apimSku string = 'Developer'

@description('Publisher email for API Management (required if deployApim is true)')
param apimPublisherEmail string = ''

@description('Publisher name for API Management')
param apimPublisherName string = 'BestBot Team'

@description('Whether to enable VNet integration for APIM')
param enableApimVnet bool = false

// Derived names
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)
var planName = 'az-asp-${resourceToken}'
var aiName = 'az-ai-${resourceToken}'
var siteName = 'az-func-${resourceToken}'
var uamiName = 'az-umi-${resourceToken}'
var lawName = 'az-law-${resourceToken}'
var apimName = 'az-apim-${resourceToken}'
var storageBase = toLower(replace('azst${resourceToken}', '-', ''))
var storageName = length(storageBase) > 24 ? substring(storageBase, 0, 24) : storageBase

// Storage account for Functions
resource stg 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    // Prefer Entra ID auth for storage
    defaultToOAuthAuthentication: true
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    supportsHttpsTrafficOnly: true
  }
  tags: tags
}

// Blob container for Flex Consumption deployment package
var deploymentContainerName = 'deploy-${resourceToken}'
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${stg.name}/default/${deploymentContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

// Log Analytics workspace for diagnostics
resource law 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: lawName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
  tags: tags
}

// Application Insights (workspace-based)
resource appi 'Microsoft.Insights/components@2020-02-02' = {
  name: aiName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: law.id
  }
  tags: tags
}

// User-assigned managed identity for Function App
resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: uamiName
  location: location
  tags: tags
}

// Flex Consumption plan
resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true // Linux
  }
  tags: tags
}

// Function App on Flex Consumption
resource site 'Microsoft.Web/sites@2024-04-01' = {
  name: siteName
  location: location
  kind: 'functionapp,linux'
  dependsOn: [
    blobContainer
  ]
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${uami.id}': {}
    }
  }
  properties: {
    httpsOnly: true
    serverFarmId: plan.id
    siteConfig: {
      appSettings: [
        // Identity-based AzureWebJobsStorage connection (host storage)
        // Use UAMI; specify account name and identity clientId for the connection
        {
          name: 'AzureWebJobsStorage__accountName'
          value: stg.name
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: uami.properties.clientId
        }
        {
          name: 'AzureWebJobsStorage__blobServiceUri'
          value: stg.properties.primaryEndpoints.blob
        }
        // App Insights
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appi.properties.ConnectionString
        }
      ]
      cors: {
        allowedOrigins: [
          '*'
        ]
        supportCredentials: false
      }
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      scmIpSecurityRestrictionsUseMain: true
      // Configure IP restrictions to only allow APIM access when deployed
      ipSecurityRestrictions: deployApim ? [
        {
          ipAddress: 'ApiManagement'
          action: 'Allow'
          priority: 100
          name: 'Allow APIM'
          description: 'Allow traffic from Azure API Management'
        }
        {
          ipAddress: 'Any'
          action: 'Deny'
          priority: 200
          name: 'Deny all other traffic'
          description: 'Deny all other traffic when APIM is enabled'
        }
      ] : [
        {
          ipAddress: 'Any'
          action: 'Allow'
          priority: 100
          name: 'Allow all traffic'
          description: 'Allow all traffic when APIM is not enabled'
        }
      ]
    }
    functionAppConfig: {
      deployment: {
        storage: {
          // Use blob container as the deployment source for OneDeploy
          type: 'blobContainer'
          // Use the UAMI to access the deployment container
          authentication: {
            type: 'UserAssignedIdentity'
            userAssignedIdentityResourceId: uami.id
          }
          // URL of the container, e.g. https://<account>.blob.core.windows.net/<container>
          value: '${stg.properties.primaryEndpoints.blob}${deploymentContainerName}'
        }
      }
      scaleAndConcurrency: {
        // Allowed values: 512, 2048, 4096 (MB)
        instanceMemoryMB: 2048
        // Max instances for Flex Consumption (allowed 40-1000)
        maximumInstanceCount: 40
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '9.0'
      }
    }
  }
  // Add service tag only to the function app for azd deploy targeting
  tags: union(tags, {
    'azd-service-name': 'functionapp'
  })
}

// Diagnostics for function app to Log Analytics
resource diag 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'azdiag'
  scope: site
  properties: {
    workspaceId: law.id
    logs: [
      {
        category: 'FunctionAppLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

// Tag the resource group with azd-env-name for azd tooling
resource rgTags 'Microsoft.Resources/tags@2021-04-01' = {
  name: 'default'
  properties: {
    tags: tags
  }
}

// Role assignments for UAMI against Storage (Blob/Table/Queue)
resource raBlobOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(stg.id, 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b', uami.id)
  scope: stg
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
    ) // Storage Blob Data Owner
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource raBlobContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(stg.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe', uami.id)
  scope: stg
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    ) // Storage Blob Data Contributor
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource raQueueContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(stg.id, '974c5e8b-45b9-4653-ba55-5f855dd0fb88', uami.id)
  scope: stg
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
    ) // Storage Queue Data Contributor
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource raTableContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(stg.id, '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3', uami.id)
  scope: stg
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'
    ) // Storage Table Data Contributor
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Metrics publisher on the Function App
resource raMetrics 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, '3913510d-42f4-4e42-8a64-420c390055eb', uami.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '3913510d-42f4-4e42-8a64-420c390055eb'
    ) // Monitoring Metrics Publisher
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Role assignment for APIM managed identity to access Function App (when APIM is deployed)
resource raApimFunctionApp 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployApim) {
  name: guid(site.id, 'de139f84-1756-47ae-9be6-808fbbe84772', uami.id, 'apim')
  scope: site
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'de139f84-1756-47ae-9be6-808fbbe84772'
    ) // Website Contributor
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Deploy API Management (conditional)
module apimModule 'modules/apim.bicep' = if (deployApim) {
  name: 'apim-deployment'
  params: {
    location: location
    tags: tags
    apimName: apimName
    apimSku: apimSku
    publisherEmail: apimPublisherEmail
    publisherName: apimPublisherName
    functionAppName: site.name
    managedIdentityId: uami.id
    enableVnet: enableApimVnet
  }
}

// Outputs for azd wiring
output functionAppName string = site.name
output functionAppResourceId string = site.id
output applicationInsightsConnectionString string = appi.properties.ConnectionString
output storageAccountName string = stg.name
output RESOURCE_GROUP_ID string = resourceGroup().id

// APIM outputs (conditional)
output apimDeployed bool = deployApim
output apimName string = deployApim && apimModule != null ? apimModule.outputs.apimName : ''
output apimGatewayUrl string = deployApim && apimModule != null ? apimModule.outputs.apimGatewayUrl : ''
output apimManagementUrl string = deployApim && apimModule != null ? apimModule.outputs.apimManagementUrl : ''
output apimDeveloperPortalUrl string = deployApim && apimModule != null ? apimModule.outputs.apimDeveloperPortalUrl : ''
output mcpEndpoint string = deployApim && apimModule != null ? apimModule.outputs.mcpEndpoint : 'https://${site.name}.azurewebsites.net'
