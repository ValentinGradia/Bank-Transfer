using Bank.Transaction.WebAPI.Application.Database;
using Bank.Transaction.WebAPI.Application.External;
using Bank.Transaction.WebAPI.Domain.Constants;
using Bank.Transaction.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json;

namespace Bank.Transaction.WebAPI.Application.Features.Process;

public class ProcessService : IProcessService
{
    private readonly IDatabaseService _databaseService;
    private readonly IServiceBusSenderService _serviceBusSenderService;
    public ProcessService(IDatabaseService databaseService, IServiceBusSenderService serviceBusSenderService)
    {
        this._databaseService = databaseService;
        _serviceBusSenderService = serviceBusSenderService;
    }

    //This method is responsible for reciving events and processing them calling the appropiate method.  
    public async Task Execute(string message, string subscription)
    {
        switch (subscription)
        {
            case ReceiveFromTopicConstants.TRANSACTION_INITIATED:
                await TransactionInitiated(message);
                break;
            case ReceiveFromTopicConstants.BALANCE_CONFIRMED:
                await BalanceConfirmed(message);
                break;
            case ReceiveFromTopicConstants.BALANCE_FAILED:
                await BalanceFailed(message);
                break;
            case ReceiveFromTopicConstants.TRANSFER_CONFIRMED:
                await TransferConfirmed(message);
                break;
            case ReceiveFromTopicConstants.TRANSFER_FAILED:
                await TransferFailed(message);
                break;
        }
    }

    
    private async Task TransactionInitiated(string message)
    {
        var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
        entity.CurrentState = CurrentStateConstants.PENDING;
        
        //We save the TransactionEntity in the database.
        var saveEntity = await ProcessDatabase(entity);

        
        //We send the event to the topic, if the saveEntity.Id is 0, it means that the transaction failed and we send the event to the TRANSACTION_FAILED subscription,
        //otherwise we send it to the BALANCE_INITIATED subscription.
        if (saveEntity.Id != 0)
        {
            var eventModel = new
            {
                saveEntity.CorrelationId,
                saveEntity.CustomerId
            };

            
            //MS Balance
            await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.BALANCE_INITIATED);
        }
        else
        {
            var eventModel = new
            {
                saveEntity.CorrelationId,
                saveEntity.CustomerId
            };

            //MS Notification
            await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSACTION_FAILED);
        }
    }

    private async Task BalanceConfirmed(string message)
    {
        var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
        entity.CurrentState = CurrentStateConstants.PENDING;
        
        var saveEntity = await ProcessDatabase(entity);
        
        var eventModel = new
        {
            saveEntity.CorrelationId,
            saveEntity.Amount,
            saveEntity.SourceAccount,
            saveEntity.DestinationAccount,
            saveEntity.CustomerId
        };

        
        //MS Transfer
        await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSFER_INITIATED);
        
    }

    private async Task BalanceFailed(string message)
    {
        var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
        entity.CurrentState = CurrentStateConstants.CANCELED;
        
        var saveEntity = await ProcessDatabase(entity);
        
        var eventModel = new
        {
            saveEntity.CorrelationId,
            saveEntity.Amount,
            saveEntity.CustomerId
        };

        //MS Notification
        await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSACTION_FAILED);
        
    }

    private async Task TransferConfirmed(string message)
    {
        var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
        entity.CurrentState = CurrentStateConstants.COMPLETED;
        
        var saveEntity = await ProcessDatabase(entity);
        
        var eventModel = new
        {
            saveEntity.CorrelationId,
            saveEntity.Amount,
            saveEntity.CustomerId
        };

        // //MS Notification
        // await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSACTION_COMPLETED);
        
        //MS Balance
        await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSFER_CONFIRMED_BALANCE);
    }

    private async Task TransferFailed(string message)
    {
        var entity = JsonConvert.DeserializeObject<TransactionEntity>(message);
        entity.CurrentState = CurrentStateConstants.CANCELED;
        
        var saveEntity = await ProcessDatabase(entity);
        
        var eventModel = new
        {
            saveEntity.CorrelationId,
            saveEntity.Amount,
            saveEntity.CustomerId
        };

        //MS Notification
        //await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSACTION_FAILED);
        
        //MS Balance
        await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSFER_FAILED_BALANCE);
    }

    public async Task<TransactionEntity> ProcessDatabase(TransactionEntity entity)
    {
        var existEntity =
            await _databaseService.Transaction.FirstOrDefaultAsync(x => x.CorrelationId == entity.CorrelationId);

        if (existEntity == null)
        {
            entity.TransactionDate = DateTime.UtcNow;
            await _databaseService.Transaction.AddAsync(entity);
            await _databaseService.SaveAsync();
            return entity;
        }
        else
        {
            existEntity.TransactionDate = DateTime.UtcNow;
            existEntity.CurrentState = entity.CurrentState;
            _databaseService.Transaction.Update(existEntity);
            await _databaseService.SaveAsync();
            return existEntity;
        }
    }
}