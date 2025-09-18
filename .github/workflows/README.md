# GitHub Actions Workflows

This directory contains GitHub Actions workflows for the BestBot project.

## Workflows

### `ci.yml` - Continuous Integration
- **Triggers**: Push to main, Pull requests to main
- **Purpose**: Validates code builds, dependencies restore, and formatting
- **Steps**:
  - Setup .NET 9
  - Restore dependencies
  - Build project
  - Verify code formatting
  - Validate publish configuration

### `azd-deploy.yml` - Azure Developer CLI Deployment
- **Triggers**: Manual dispatch only (disabled by default)
- **Purpose**: Deploy the application to Azure using Azure Developer CLI
- **Requirements**: 
  - Azure service principal with proper permissions
  - Environment variables configured in GitHub repository settings
- **Steps**:
  - Build and validate (same as CI)
  - Install Azure Developer CLI
  - Authenticate with Azure
  - Provision Azure resources using Bicep templates
  - Deploy the function app

## Configuration Requirements

### For AZD Deployment (`azd-deploy.yml`)

#### Repository Variables (Settings → Secrets and variables → Actions → Variables)
- `AZURE_CLIENT_ID`: Client ID of the Azure service principal
- `AZURE_TENANT_ID`: Azure Active Directory tenant ID  
- `AZURE_SUBSCRIPTION_ID`: Target Azure subscription ID
- `AZURE_ENV_NAME`: Environment name for azd (e.g., "dev", "prod")

#### Repository Secrets (Settings → Secrets and variables → Actions → Secrets)
- `AZD_INITIAL_ENVIRONMENT_CONFIG`: Initial environment configuration (optional)

#### Azure Service Principal Setup
1. Create a service principal:
   ```bash
   az ad sp create-for-rbac --name "BestBot-GitHub" --role contributor --scopes /subscriptions/{subscription-id} --json-auth
   ```
2. Assign additional permissions as needed for resource group and storage account creation
3. Configure federated credentials for GitHub Actions OIDC

#### Enabling the Deployment Workflow
To enable automatic deployment:
1. Edit `azd-deploy.yml`
2. Uncomment the push/pull_request triggers
3. Remove the `workflow_dispatch` trigger if desired
4. Ensure all required variables and secrets are configured

## Local Development
See the main README.md for local development setup instructions.