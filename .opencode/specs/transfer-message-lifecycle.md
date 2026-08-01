# Add Message Lifecycle Infrastructure to Transfer Microservice

## Why

The Transfer microservice currently only has domain constants, entity, and database persistence, but lacks the Service Bus message lifecycle infrastructure (sender, receiver, process handler, process service). Without these, Transfer cannot participate in the saga — it cannot receive `transfer-initiated` events or publish `transfer-confirmed`/`transfer-failed` events. This is the same infrastructure already working in Balance and Transaction.

## What

Add 7 new files and update `Program.cs` so Transfer can:
1. Listen to `transfer-initiated` messages from the Service Bus topic
2. Route incoming messages through MediatR to a ProcessHandler
3. Execute transfer logic (save to DB, update state)
4. Publish `transfer-confirmed` or `transfer-failed` events back to the topic

## Constraints

### Must

- Follow the exact same patterns as Balance and Transaction microservices (same file structure, same DI registration approach)
- Use MediatR for message routing (ProcessHandler → IProcessService)
- Use `IServiceProvider.CreateScope()` in ProcessHandler to avoid captive dependency (singleton handler → scoped service)
- Register `ServiceBusReceiveService` as `AddHostedService`
- Register `IServiceBusSenderService` as singleton
- Register `IProcessService` as scoped

### Must Not

- Do not modify any existing files in Transaction, Balance, or Gateway
- Do not change Transfer's existing constants, entity, or database files

### Out of Scope

- Actual transfer business logic beyond state persistence (placeholder methods)

## Current State

Transfer has the skeleton but no message infrastructure:

- **Exists:** `Program.cs` (minimal — only DB registration), `ReceiveFromTopicConstants.cs` (`TRANSFER_INITIATED`), `SendToTopicConstants.cs` (`TRANSFER_CONFIRMED`, `TRANSFER_FAILED`), `CurrentStateConstants.cs` (`PENDING`), `TransferEntity.cs`, `IDatabaseService.cs`, `DatabaseService.cs`
- **Missing:** ProcessHandler, IProcessService, ProcessService, IServiceBusSenderService, ServiceBusSenderService, ServiceBusReceiveService, ProcessEvent
- **Reference implementation:** `src/Bank.Balance/Bank.Balance.WebAPI/` (same pattern, different business logic)

## Tasks

### T0: Add required NuGet packages to Transfer.csproj

What: Add the same packages Balance.csproj uses:
- `Azure.Messaging.ServiceBus`
- `MediatR`
- `Newtonsoft.Json`

Files: `src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj`

Verify: `dotnet restore src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj` succeeds

### T1: Create message lifecycle infrastructure files

What: Create all 7 missing files by copying the Balance and Transaction pattern and adapting namespaces and constants for Transfer:

1. `Domain/Events/ProcessEvent.cs` — MediatR notification (identical to Balance, change namespace)
2. `Application/External/IServiceBusSenderService.cs` — Interface (identical to Balance, change namespace)
3. `External/ServiceBusSender/ServiceBusSenderService.cs` — Sender implementation (identical to Balance, change namespace)
4. `Application/Handlers/ProcessHandler.cs` — MediatR handler with scope creation (identical to Balance, change namespace)
5. `Application/Features/Process/IProcessService.cs` — Interface (identical to Balance, change namespace)
6. `Application/Features/Process/ProcessService.cs` — Business logic: receives `TRANSFER_INITIATED`, saves to DB, publishes `TRANSFER_CONFIRMED` or `TRANSFER_FAILED`
7. `External/ServiceBusReceive/ServiceBusReceiveService.cs` — BackgroundService listener for `TRANSFER_INITIATED` subscription

Files: All 7 new files under `src/Bank.Transfer/Bank.Transfer.WebAPI/`

Verify: `dotnet restore src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj && dotnet build src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj` succeeds

### T2: Update Program.cs with DI registrations

What: Add Service Bus, MediatR, and handler registrations to `Program.cs`, following the Balance pattern:
- Add `using` statements for new namespaces
- Register `IProcessService` as scoped
- Register `IServiceBusSenderService` as singleton
- Register `ServiceBusReceiveService` as hosted service
- Register MediatR with `ProcessHandler` assembly

Files: `src/Bank.Transfer/Bank.Transfer.WebAPI/Program.cs`

Verify: `dotnet restore src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj && dotnet build src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj` succeeds

## Validation

- `dotnet restore src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj && dotnet build src/Bank.Transfer/Bank.Transfer.WebAPI/Bank.Transfer.WebAPI.csproj` — builds without errors
- Check whether the life cycle of a transfer message will be handled correctly
- Manual check: compare file structure of Transfer with Balance and Transaction — should mirror the same External/, Application/Handlers/, Application/Features/Process/ layout
- Manual check: `Program.cs` registers all services matching Balance's registration pattern
