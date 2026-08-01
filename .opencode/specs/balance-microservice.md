# Balance Microservice — Service Bus Integration

## Why

The Balance microservice is scaffolded (entity, DB context, constants) but has no messaging or business logic. It needs to participate in the saga by receiving `balance-initiated` events and responding with `balance-confirmed` or `balance-failed`. Without this, the Transaction service sends `balance-initiated` into the void and the saga stalls.

## What

Wire up the Balance microservice to:
1. Receive `balance-initiated` from Service Bus
2. Process the balance (upsert to DB with `BalanceDate` set)
3. Publish `balance-confirmed` (success) or `balance-failed` (failure) back to the topic
4. Receive `transfer-confirmed-balance` and `transfer-failed-balance` (stub handlers for future)

## Constraints

### Must

- Follow the exact same patterns as Bank.Transaction.WebAPI (ServiceBusReceiveService, ProcessHandler, ProcessService, ServiceBusSenderService)
- Use MediatR for internal event routing (ServiceBusReceiveService → ProcessHandler → ProcessService)
- Use Newtonsoft.Json for deserialization (matching Transaction)
- Use System.Text.Json for serialization in ServiceBusSenderService (matching Transaction)
- Create a DI scope in ProcessHandler to resolve scoped services (captive dependency fix)
- Register ServiceBusReceiveService as a BackgroundService
- Register ServiceBusSenderService as Singleton
- Register ProcessService as Scoped
- Read `SERVICEBUSCONSTR` and `SERVICEBUSTOPIC` from User Secrets
- Read `Database` connection string from User Secrets
- Set `BalanceDate = DateTime.UtcNow` on confirmed balances
- Upsert by `CorrelationId` (same pattern as Transaction.ProcessDatabase)

### Must Not

- Do NOT add HTTP endpoints or controllers (event-driven only)
- Do NOT add new NuGet packages beyond Service Bus, MediatR, and Newtonsoft.Json
- Do NOT modify any files outside the Balance project
- Do NOT modify existing entity, constants, or DB context files
- Do NOT implement business logic for `transfer-confirmed-balance` or `transfer-failed-balance` beyond empty stubs
- Do NOT add validation logic beyond the basic null/success check

### Out of Scope

- Transfer service implementation
- Transaction service `BalanceConfirmed`/`BalanceFailed` handler implementations
- Balance API endpoints
- Unit tests

## Current State

**Balance project:** `src/Bank.Balance/Bank.Balance.WebAPI/`

Already exists:
- `Domain/Entities/BalanceEntity.cs` — Entity with Id, CorrelationId, BalanceDate, CurrentState, CustomerId
- `Domain/Constants/SendToTopicConstants.cs` — BALANCE_CONFIRMED, BALANCE_FAILED, TRANSFER_INITIATED
- `Domain/Constants/ReceiveFromTopicConstants.cs` — BALANCE_INITIATED, TRANSFER_CONFIRMED_BALANCE, TRANSFER_FAILED_BALANCE
- `Domain/Constants/CurrentStateConstants.cs` — PENDING, COMPLETED, CANCELED
- `Application/Database/IDatabaseService.cs` — DbSet<BalanceEntity> + SaveAsync()
- `Peristence/Database/DatabaseService.cs` — DbContext implementation
- `Program.cs` — EF Core registration only
- `.csproj` — No Service Bus or MediatR packages

**Reference project:** `src/Bank.Transaction/Bank.Transaction.WebAPI/`

Follow these files exactly:
- `External/ServiceBusReceive/ServiceBusReceiveService.cs`
- `External/ServiceBusSender/ServiceBusSenderService.cs`
- `Application/Handlers/ProcessHandler.cs`
- `Application/Features/Process/ProcessService.cs`
- `Domain/Events/ProcessEvent.cs`

**Event flow context:**
- Transaction sends `balance-initiated` with `{ CorrelationId, CustomerId }`
- Balance receives it, processes, and sends `balance-confirmed` or `balance-failed`
- Transaction also sends `transfer-confirmed-balance` / `transfer-failed-balance` (future)

## Tasks

### T1: Add NuGet packages and create sender service

What: Add Service Bus, MediatR, and Newtonsoft.Json packages to `.csproj`. Create `IServiceBusSenderService` interface and `ServiceBusSenderService` implementation (Singleton).

Files:
- `Bank.Balance.WebAPI.csproj` (modify)
- `Application/External/IServiceBusSenderService.cs` (create)
- `External/ServiceBusSender/ServiceBusSenderService.cs` (create)

Verify: `dotnet build` succeeds

### T2: Create MediatR event and receiver service

What: Create `ProcessEvent` (MediatR notification), `ProcessHandler` (notification handler with DI scope), and `ServiceBusReceiveService` (BackgroundService listening to BALANCE_INITIATED, TRANSFER_CONFIRMED_BALANCE, TRANSFER_FAILED_BALANCE).

Files:
- `Domain/Events/ProcessEvent.cs` (create)
- `Application/Handlers/ProcessHandler.cs` (create)
- `External/ServiceBusReceive/ServiceBusReceiveService.cs` (create)

Verify: `dotnet build` succeeds

### T3: Create ProcessService with balance handlers

What: Create `IProcessService` and `ProcessService` with the switch router. Implement `BalanceInitiated` (deserialize with Newtonsoft.Json, upsert to DB with BalanceDate = DateTime.UtcNow, send balance-confirmed or balance-failed). Add empty stubs for `TransferConfirmedBalance` and `TransferFailedBalance`.

Files:
- `Application/Features/Process/IProcessService.cs` (create)
- `Application/Features/Process/ProcessService.cs` (create)

Verify: `dotnet build` succeeds

### T4: Wire up Program.cs and configure User Secrets

What: Register all services in `Program.cs` with their interfaces:
- `AddScoped<IDatabaseService, DatabaseService>()`
- `AddScoped<IProcessService, ProcessService>()`
- `AddSingleton<IServiceBusSenderService, ServiceBusSenderService>()`
- `AddHostedService<ServiceBusReceiveService>()`
- `AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(ProcessHandler).Assembly))`
- Add EF Core with SQL Server (`Database` connection string)

Also configure User Secrets with `SERVICEBUSCONSTR`, `SERVICEBUSTOPIC`, and `Database` connection string.
Files:
- `Program.cs` (modify)
- User Secrets (configure via CLI)

Verify: `dotnet build` succeeds, `dotnet user-secrets list` shows all 3 secrets

## Validation

- `dotnet build src/Bank.Balance/Bank.Balance.WebAPI/` passes
- `dotnet user-secrets list` shows SERVICEBUSCONSTR, SERVICEBUSTOPIC, Database
