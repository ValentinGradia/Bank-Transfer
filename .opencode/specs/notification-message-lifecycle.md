# Notification Microservice — Service Bus Integration

## Why

The Notification microservice is scaffolded (entity, DB context, constants) but has no messaging or business logic. It needs to participate in the saga by receiving `transaction-completed` and `transaction-failed` events and persisting notifications to Cosmos DB. Without this, the Transaction service publishes completion/failure events into the void and users receive no notifications.

## What

Wire up the Notification microservice to:
1. Receive `transaction-completed` and `transaction-failed` from Service Bus
2. Process notifications (save to Cosmos DB with NotificationDate set)
3. **No outgoing events** — this service is a terminal consumer in the saga

## Constraints

### Must

- Follow the same patterns as Balance and Transaction (ServiceBusReceiveService, ProcessHandler, ProcessService) excepts of ServiceBusSenderService (Notification does not send events)
- Use MediatR for internal event routing (ServiceBusReceiveService → ProcessHandler → ProcessService)
- Use Newtonsoft.Json for deserialization (matching other microservices)
- Create a DI scope in ProcessHandler to resolve scoped services (captive dependency fix)
- Register ServiceBusReceiveService as a BackgroundService
- Register ProcessService as Scoped
- Read `SERVICEBUSCONSTR` and `SERVICEBUSTOPIC` from User Secrets
- Read `CosmosDb`, `NotificationDBName`, `NotificationDBContainer` from User Secrets
- Set `NotificationDate = DateTime.UtcNow` on saved notifications (already handled by DatabaseService)
- Use existing Cosmos DB persistence (DatabaseService.AddNotificationAsync)

### Must Not

- Do NOT add HTTP endpoints or controllers (event-driven only)
- Do NOT create ServiceBusSenderService (Notification never publishes events)
- Do NOT add `IServiceBusSenderService` interface or implementation
- Do NOT add new NuGet packages beyond Service Bus and MediatR (Newtonsoft.Json already present)
- Do NOT modify any files outside the Notification project
- Do NOT modify existing entity, constants, or database service files
- Do NOT add validation logic beyond basic null checks

### Out of Scope

- Notification API endpoints
- Unit tests
- Notification templates or formatting logic

## Current State

**Notification project:** `src/Bank.Notification/Bank.Notification.WebAPI/`

Already exists:
- `Domain/Entities/NotificationEntity.cs` — Entity with Id, CorrelationId, NotificationDate, CustomerId, Type, Content, TransactionStatus (Cosmos DB JSON properties)
- `Domain/Constants/ReceiveFromTopicConstants.cs` — TRANSACTION_FAILED, TRANSACTION_COMPLETED
- `Application/Database/IDatabaseService.cs` — AddNotificationAsync(), GetAllNotificationsAsync()
- `Persistence/DatabaseService.cs` — Cosmos DB implementation (CreateItemAsync with partition key)
- `Program.cs` — Basic setup with IDatabaseService registration only
- `.csproj` — Has Newtonsoft.Json, Microsoft.Azure.Cosmos; missing Service Bus and MediatR

**Reference projects:**
- `src/Bank.Balance/Bank.Balance.WebAPI/` — Same pattern but with sender (we skip sender)
- `src/Bank.Transaction/Bank.Transaction.WebAPI/` — Full implementation reference

**Event flow context:**
- Transaction publishes `transaction-completed` with correlation data
- Transaction publishes `transaction-failed` with correlation data
- Notification receives both and saves to Cosmos DB

## Tasks

### T1: Add NuGet packages and create receiver infrastructure

What: Add `Azure.Messaging.ServiceBus` and `MediatR` packages to `.csproj`. Create `ProcessEvent` (MediatR notification) and `ProcessHandler` (notification handler with DI scope).

Files:
- `Bank.Notification.WebAPI.csproj` (modify — add Service Bus and MediatR)
- `Domain/Events/ProcessEvent.cs` (create)
- `Application/Handlers/ProcessHandler.cs` (create)

Verify: `dotnet build` succeeds

### T2: Create ServiceBusReceiveService

What: Create `ServiceBusReceiveService` BackgroundService that listens to `TRANSACTION_COMPLETED` and `TRANSACTION_FAILED` subscriptions (from `ReceiveFromTopicConstants`). Follow the exact pattern from Balance/Transaction but only implement receive (no send).

Files:
- `External/ServiceBusReceive/ServiceBusReceiveService.cs` (create)

Verify: `dotnet build` succeeds

### T3: Create ProcessService with notification handlers

What: Create `IProcessService` and `ProcessService` with the switch router. **ProcessService constructor takes only `IDatabaseService` (no sender service — Notification is receive-only).**

Implement `TransactionCompleted` and `TransactionFailed` handlers:
- Deserialize with Newtonsoft.Json into `NotificationEntity`
- Set `CorrelationId` and `CustomerId` from incoming message
- Set `TransactionStatus` to `true` for completed, `false` for failed
- Set `Type` to the event name (e.g., `"transaction-completed"`)
- Set `Content` to the raw message body
- `Id` and `NotificationDate` are handled by `DatabaseService.AddNotificationAsync()`
- Save to Cosmos DB using existing `IDatabaseService.AddNotificationAsync()`

Files:
- `Application/Features/Process/IProcessService.cs` (create)
- `Application/Features/Process/ProcessService.cs` (create)

Verify: `dotnet build` succeeds

### T4: Wire up Program.cs and configure User Secrets

What: Register all services in `Program.cs` with their interfaces:
- `AddScoped<IProcessService, ProcessService>()`
- `AddHostedService<ServiceBusReceiveService>()`
- `AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly))`
- Ensure `IDatabaseService` remains registered (already exists)

Note: `Application\Features\` folder placeholder in `.csproj` can be removed after T3 creates files there. Leave `Application\External\` as-is (empty).

Also configure User Secrets with `SERVICEBUSCONSTR`, `SERVICEBUSTOPIC`, `CosmosDb`, `NotificationDBName`, and `NotificationDBContainer`. Note: Cosmos DB secrets may already exist from previous setup — verify before overwriting.

Files:
- `Program.cs` (modify)
- User Secrets (configure via CLI)

Verify: `dotnet build` succeeds, `dotnet user-secrets list` shows all 5 secrets

## Validation

- `dotnet build src/Bank.Notification/Bank.Notification.WebAPI/` passes
- `dotnet user-secrets list` shows SERVICEBUSCONSTR, SERVICEBUSTOPIC, CosmosDb, NotificationDBName, NotificationDBContainer
- Manual check: compare file structure with Balance — should mirror same Application/Handlers/, Application/Features/Process/, External/ServiceBusReceive/ layout (minus ServiceBusSender)
- Manual check: `Program.cs` registers all services matching Balance's registration pattern (minus sender)
