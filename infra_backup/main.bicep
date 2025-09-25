// Simplified infrastructure focused on Azure Functions Flex Consumption with private networking
// and zip deploy via the platform SCM endpoint.

@description('Azure region to deploy to')
param location string = resourceGroup().location

@description('Azd environment name')
param environmentName string

@description('Tags to apply to resources')
param tags object = {
  'azd-env-name': environmentName
}

@description('Virtual network address space')
param vnetAddressSpace string = '10.42.0.0/16'

@description('Subnet prefix for Azure Functions integration (delegated to Microsoft.Web/serverFarms)')
param functionSubnetPrefix string = '10.42.1.0/24'

@description('Subnet prefix for private endpoints')
param privateEndpointsSubnetPrefix string = '10.42.10.0/24'

@description('Instance memory size for Flex Consumption plan (MB). Supported values: 512, 2048, 4096')
@allowed([512, 2048, 4096])
param instanceMemoryMB int = 512

@description('Maximum number of Flex Consumption instances (allowed range 40-1000).')
@minValue(40)
@maxValue(1000)
param maximumInstanceCount int = 40

var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)
var planName = 'az-asp-${resourceToken}'
var aiName = 'az-ai-${resourceToken}'
var siteName = 'az-func-${resourceToken}'
var uamiName = 'az-umi-${resourceToken}'
var lawName = 'az-law-${resourceToken}'
var storageBase = toLower(replace('azst${resourceToken}', '-', ''))
var storageName = length(storageBase) > 24 ? substring(storageBase, 0, 24) : storageBase
var deploymentContainerName = 'deploy-${resourceToken}'
var vnetName = 'vnet-${resourceToken}'
var functionSubnetId = resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, 'function')
var privateEndpointSubnetId = resourceId('Microsoft.Network/virtualNetworks/subnets', vnetName, 'private-endpoints')
var storageDnsSuffix = environment().suffixes.storage

var storagePrivateDnsZones = [
  {
    name: 'privatelink.blob.${storageDnsSuffix}'
    group: 'blob'
  }
  {
    name: 'privatelink.queue.${storageDnsSuffix}'
    group: 'queue'
  }
  {
    name: 'privatelink.table.${storageDnsSuffix}'
    group: 'table'
  }
]

// Storage account locked to private endpoints only
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
    defaultToOAuthAuthentication: true
    supportsHttpsTrafficOnly: true
    publicNetworkAccess: 'Disabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
    }
  }
  tags: tags
}

// Deployment package container for Flex Consumption
resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${stg.name}/default/${deploymentContainerName}'
  properties: {
    publicAccess: 'None'
  }
}

// Log Analytics workspace
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

// User-assigned managed identity for Function <-> Storage interactions
resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: uamiName
  location: location
  tags: tags
}

// Virtual network with delegated subnet for Functions and subnet for private endpoints
resource vnet 'Microsoft.Network/virtualNetworks@2023-09-01' = {
  name: vnetName
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        vnetAddressSpace
      ]
    }
    subnets: [
      {
        name: 'function'
        properties: {
          addressPrefix: functionSubnetPrefix
          delegations: [
            {
              name: 'functions-delegation'
              properties: {
                serviceName: 'Microsoft.Web/serverFarms'
              }
            }
          ]
        }
      }
      {
        name: 'private-endpoints'
        properties: {
          addressPrefix: privateEndpointsSubnetPrefix
          privateEndpointNetworkPolicies: 'Disabled'
          privateLinkServiceNetworkPolicies: 'Enabled'
        }
      }
    ]
  }
  tags: tags
}

// Private DNS zones for storage services
resource dnsZones 'Microsoft.Network/privateDnsZones@2020-06-01' = [
  for zone in storagePrivateDnsZones: {
    name: zone.name
    location: 'global'
    properties: {}
  }
]

// Link VNet to each private DNS zone
resource dnsZoneLinks 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = [
  for (zone, i) in storagePrivateDnsZones: {
    name: '${dnsZones[i].name}/${vnet.name}-link'
    location: 'global'
    properties: {
      registrationEnabled: false
      virtualNetwork: {
        id: vnet.id
      }
    }
  }
]

// Private endpoints per storage subresource (blob/queue/table)
resource storagePrivateEndpoints 'Microsoft.Network/privateEndpoints@2023-09-01' = [
  for (zone, i) in storagePrivateDnsZones: {
    name: 'pe-${zone.group}-${resourceToken}'
    location: location
    properties: {
      subnet: {
        id: privateEndpointSubnetId
      }
      privateLinkServiceConnections: [
        {
          name: 'storage-${zone.group}'
          properties: {
            privateLinkServiceId: stg.id
            groupIds: [
              zone.group
            ]
          }
        }
      ]
    }
    tags: tags
  }
]

// Attach DNS zones to each private endpoint
resource storagePrivateEndpointDnsGroups 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2020-05-01' = [
  for (zone, i) in storagePrivateDnsZones: {
    name: '${storagePrivateEndpoints[i].name}/zone-group'
    properties: {
      privateDnsZoneConfigs: [
        {
          name: 'storage-${zone.group}'
          properties: {
            privateDnsZoneId: dnsZones[i].id
          }
        }
      ]
    }
  }
]

// Flex Consumption plan
resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: planName
  location: location
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
  tags: tags
}

// Function App configured for VNet integration and identity-based storage access
resource site 'Microsoft.Web/sites@2024-04-01' = {
  name: siteName
  location: location
  kind: 'functionapp,linux'
  dependsOn: [
    deploymentContainer
  ]
  identity: {
    type: 'SystemAssigned'
    // userAssignedIdentities: {
    //   '${uami.id}': {}
    // }
  }
  properties: {
    httpsOnly: true
    serverFarmId: plan.id
    virtualNetworkSubnetId: functionSubnetId
    reserved: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: stg.name
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        // {
        //   name: 'AzureWebJobsStorage__clientId'
        //   value: uami.properties.clientId
        // }
        // {
        //   name: 'AzureWebJobsStorage__blobServiceUri'
        //   value: stg.properties.primaryEndpoints.blob
        // }
        // {
        //   name: 'AzureWebJobsStorage__queueServiceUri'
        //   value: stg.properties.primaryEndpoints.queue
        // }
        // {
        //   name: 'AzureWebJobsStorage__tableServiceUri'
        //   value: stg.properties.primaryEndpoints.table
        // }
        // {
        //   name: 'AzureWebJobsStorage__accountResourceId'
        //   value: stg.id
        // }
        // {
        //   name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
        //   value: appi.properties.ConnectionString
        // }
        // {
        //   name: 'APPLICATIONINSIGHTS_AUTHENTICATION_STRING'
        //   value: 'Authorization=AAD'
        // }
        // {
        //   name: 'WEBSITE_VNET_ROUTE_ALL'
        //   value: '1'
        // }
      ]
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      scmIpSecurityRestrictionsUseMain: true
    }
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${stg.properties.primaryEndpoints.blob}${deploymentContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
            // userAssignedIdentityResourceId: uami.id
          }
        }
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '9.0'
      }
      scaleAndConcurrency: {
        instanceMemoryMB: instanceMemoryMB
        maximumInstanceCount: maximumInstanceCount
      }
    }
  }
  tags: union(tags, {
    'azd-service-name': 'functionapp'
  })
}

// Diagnostic settings to Log Analytics
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

// Resource group tags (ensures tag drift correction)
resource rgTags 'Microsoft.Resources/tags@2021-04-01' = {
  name: 'default'
  properties: {
    tags: tags
  }
}

// Role assignments for managed identity (storage data operations + metrics)
resource raBlobOwner 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(stg.id, 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b', uami.id)
  scope: stg
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
    )
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
    )
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
    )
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource raMetrics 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, '3913510d-42f4-4e42-8a64-420c390055eb', uami.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '3913510d-42f4-4e42-8a64-420c390055eb'
    )
    principalId: uami.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
output functionAppName string = site.name
output functionAppResourceId string = site.id
output zipDeployEndpoint string = 'https://${site.name}.scm.azurewebsites.net/api/zipdeploy'
output storageAccountName string = stg.name
output managedIdentityClientId string = uami.properties.clientId
output vnetNameOut string = vnet.name
output vnetId string = vnet.id
output functionSubnetResourceId string = functionSubnetId
output privateEndpointSubnetResourceId string = privateEndpointSubnetId
output privateDnsZonesOut array = [for zone in storagePrivateDnsZones: zone.name]
