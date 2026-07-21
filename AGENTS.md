# Bank Transaction Infrastructure

## Overview

This project defines Azure infrastructure for a bank transaction system using Terraform with the AzureRM provider.

## Configuration

- **Terraform Version**: >= 1.2
- **AzureRM Provider**: ~> 4.2
- **Resource Provider Registrations**: none (manual registration required)
- **Prevent Deletion**: disabled (test environment)

## Azure Resource Providers Used

| Provider | Resource Types |
|----------|---------------|
| Microsoft.Web | Service Plans, Linux Web Apps |
| Microsoft.Sql | MSSQL Servers, Databases |
| Microsoft.DocumentDB | Cosmos DB Accounts, SQL Databases |
| Microsoft.Storage | Storage Accounts |
| Microsoft.ServiceBus | Service Bus Namespaces |

## Data Sources

| Name | Type | Description |
|------|------|-------------|
| azurerm_client_config.current | azurerm_client_config | Retrieves current Azure client configuration (tenant ID, subscription ID, object ID) |

## Resource Groups

| Name | Location | Purpose |
|------|----------|---------|
| rg-transfer-eastus | eastus | Main resources |

## Service Plan

| Name | Location | OS | SKU |
|------|----------|----|-----|
| plan-apigateaway-centralus | centralus | Linux | F1 (free tier) |

## Web Applications

All apps share the same service plan (plan-apigateaway-centralus) and use nginx:latest Docker image with always_on = false.

| Resource Name | App Name | Location |
|--------------|----------|----------|
| appservice_gateaway | appserv-apigateaway-centralus | centralus |
| appservice_transaction | appserv-Mytransaction-centralus | centralus |
| appservice_balance | appserv-My-balance-centralus | centralus |
| appservice_transfer | appserv-My-transfer-centralus | centralus |
| appservice_notification | appserv-my-notification-centralus | centralus |

## Database (MSSQL)

| Resource Name | Type | Name | Location | Version | SKU |
|--------------|------|------|----------|---------|-----|
| sql_my_server | MSSQL Server | mssql-server-centralus | centralus | 12.0 | - |
| sql_my_database | MSSQL Database | sql-my-db-centralus | - | - | Basic |

## Database (Cosmos DB)

| Resource Name | Type | Name | Location | Tier | Consistency | Free Tier |
|--------------|------|------|----------|------|-------------|-----------|
| cosmosdb_account_notification | Cosmos DB Account (SQL API) | account-notification-v2-centralus | centralus | Standard | Session | enabled |
| cosmosdb_sql_notification | Cosmos DB SQL Database | cosmosdb_notification | - | - | - | - |

## Messaging

| Resource Name | Type | Name | Location | SKU |
|--------------|------|------|----------|-----|
| sb_transfer | Service Bus Namespace | servicebus-transfer-centralus-v2 | centralus | Standard |

## Storage

| Resource Name | Type | Name | Location | Tier | Replication |
|--------------|------|------|----------|------|-------------|
| storage_account_function | Storage Account | safunctioncentralus | centralus | Standard | LRS |

## Commented Resources (Not Active)

The following resources are planned but not yet present in main.tf:

- Service plan plan_deadletter � SKU: Y1/B1
- Linux function app func-deadletter-centralus

> These resources are **not active** and are not deployed.

## Commands

```bash
# Initialize Terraform
terraform init

# Plan changes
terraform plan

# Apply changes
terraform apply

# Destroy resources
terraform destroy
```

## Architecture Notes

- **Shared Service Plan**: All microservices run on a single F1 service plan. Acceptable for learning; in production, each microservice should have its own plan for independent scaling.
- **Shared SQL Database**: One database shared across all microservices. Acceptable for learning Terraform/Azure basics; in production, each microservice should own its database to avoid tight coupling.

## Environment

Test/development environment with free-tier resources and no always-on apps.

## User Secrets

All 5 microservices use .NET User Secrets to store sensitive Service Bus credentials outside of source code. The `secrets.json` file is stored locally at `%APPDATA%\microsoft\UserSecrets\<UserSecretsId>\secrets.json`.

### UserSecretsId per Project

| Project |
|---------|
| Bank.Transfer.WebAPI | 
| Bank.Transaction.WebAPI | 
| Bank.Balance.WebAPI | 
| Bank.Gateway (WebApplication1) | 
| Bank.Notification.WebAPI | 

### Stored Secrets (same across all projects)

| Key |
|-----|
| `SERVICEBUSCONSTR` | 
| `SERVICEBUSTOPIC` | 

### User Secrets CLI Commands

All commands must be run from the project directory (where the `.csproj` is located):

```bash
# Initialize user secrets (generates UserSecretsId in .csproj)
dotnet user-secrets init

# Set a secret
dotnet user-secrets set "KEY" "VALUE"

# List all secrets
dotnet user-secrets list

# Remove a secret
dotnet user-secrets remove "KEY"

# Clear all secrets
dotnet user-secrets clear
```

### Accessing Secrets in Code

```csharp
// In Program.cs - flat key access
var serviceBusConnection = builder.Configuration["SERVICEBUSCONSTR"];
var topicName = builder.Configuration["SERVICEBUSTOPIC"];

// Or bind to a settings class
builder.Services.Configure<ServiceBusSettings>(builder.Configuration);
```

> **Note:** `secrets.json` is only loaded automatically in the **Development** environment. For production, use Azure Key Vault, environment variables, or other secret management solutions.
