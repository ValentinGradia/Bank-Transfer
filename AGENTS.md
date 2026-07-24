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

## Microservice Architecture

### Overview

The system follows a **microservices architecture** with 5 independent services communicating asynchronously through Azure Service Bus. It uses the **Saga Pattern (Choreography-based)** for distributed transaction management - each service publishes events that trigger the next step, without a central orchestrator.

### Services

| Service | Project | Responsibility |
|---------|---------|----------------|
| **Gateway** | Bank.Gateway (WebApplication1) | API entry point. Receives external requests and publishes `transaction-initiated` to start the saga. |
| **Transaction** | Bank.Transaction.WebAPI | Saga coordinator. Receives events from all services, tracks transaction state, and publishes commands to drive the flow forward. |
| **Balance** | Bank.Balance.WebAPI | Handles balance checks and updates. Receives `balance-initiated`, validates/processes balance, and publishes `balance-confirmed` or `balance-failed`. Also receives transfer result notifications. |
| **Transfer** | Bank.Transfer.WebAPI | Handles fund transfers. Receives `transfer-initiated`, executes the transfer, and publishes `transfer-confirmed` or `transfer-failed`. |
| **Notification** | Bank.Notification.WebAPI | Sends notifications on transaction completion/failure. Subscribes to `transaction-completed` and `transaction-failed`. Uses Cosmos DB for persistence. |

### Event-Driven Communication (Saga Choreography)

All communication happens via **Azure Service Bus topics/subscriptions**. Services publish events to a shared topic; other services receive events through their own subscriptions.

#### Event Flow

```
External Request
       │
       ▼
┌──────────┐  transaction-initiated  ┌─────────────┐
│ Gateway  │ ────────────────────────▶│ Transaction │
└──────────┘                         └──────┬──────┘
                                            │
                              balance-initiated / transfer-initiated
                                            │
                          ┌─────────────────┼─────────────────┐
                          ▼                 │                 ▼
                   ┌──────────┐             │          ┌──────────┐
                   │ Balance  │             │          │ Transfer │
                   └────┬─────┘             │          └────┬─────┘
                        │                   │               │
           balance-confirmed/failed         │    transfer-confirmed/failed
                        │                   │               │
                        └──────┐  ◀────────┘  ─────────────┘
                               ▼
                        ┌─────────────┐  transaction-completed/failed  ┌──────────────┐
                        │ Transaction │ ──────────────────────────────▶│ Notification │
                        └─────────────┘                                └──────────────┘
```

#### Detailed Event Flow

1. **Gateway** → publishes `transaction-initiated`
2. **Transaction** receives `transaction-initiated` → publishes `balance-initiated` (and/or `transfer-initiated`)
3. **Balance** receives `balance-initiated` → publishes `balance-confirmed` or `balance-failed`
4. **Transaction** receives confirmation/failure → publishes `transfer-initiated`
5. **Transfer** receives `transfer-initiated` → publishes `transfer-confirmed` or `transfer-failed`
6. **Transaction** receives transfer result → publishes `transaction-completed` or `transaction-failed`
7. **Transaction** also publishes `transfer-confirmed-balance` / `transfer-failed-balance` (for Balance to update final state)
8. **Notification** receives `transaction-completed` or `transaction-failed`

### Transaction States

Managed via `CurrentStateConstants` (defined in Transaction, Balance, and Transfer):

| State | Description |
|-------|-------------|
| `Pending` | Transaction in progress, waiting for downstream events |
| `Completed` | Transaction succeeded |
| `Canceled` | Transaction failed or was rolled back |

### Constants Classes

These constants ensure type-safe, centralized event names across the distributed system, preventing typos and making the event-driven flow maintainable.

Each service defines constants for its publish/subscribe events:

- **`SendSubscriptionConstants`** - Event names the service publishes to the Service Bus topic.
- **`ReceivedSubscriptionsConstants`** - Event names the service subscribes to (filters from the topic).
- **`CurrentStateConstants`** - Transaction lifecycle states used to track saga progress.

> **Note:** They are currently created manually or via the .NET Service Bus SDK at runtime.

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
