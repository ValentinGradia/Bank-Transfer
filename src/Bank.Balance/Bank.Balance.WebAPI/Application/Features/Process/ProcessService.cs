using Bank.Balance.WebAPI.Application.Database;
using Bank.Balance.WebAPI.Application.External;
using Bank.Balance.WebAPI.Domain.Constants;
using Bank.Balance.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bank.Balance.WebAPI.Application.Features.Process;

public class ProcessService : IProcessService
{
    private readonly IDatabaseService _databaseService;
    private readonly IServiceBusSenderService _serviceBusSenderService;
    public ProcessService(IDatabaseService databaseService, IServiceBusSenderService serviceBusSenderService)
    {
        this._databaseService = databaseService;
        _serviceBusSenderService = serviceBusSenderService;
    }

    public async Task Execute(string message, string subscription)
    {
        switch (subscription)
        {
            case ReceiveFromTopicConstants.BALANCE_INITIATED:
                await BalanceInitiated(message);
                break;
            case ReceiveFromTopicConstants.TRANSFER_CONFIRMED_BALANCE:
                await TransferConfirmedBalance(message);
                break;
            case ReceiveFromTopicConstants.TRANSFER_FAILED_BALANCE:
                await TransferFailedBalance(message);
                break;
        }
    }

    private async Task BalanceInitiated(string message)
    {
        var entity = JsonConvert.DeserializeObject<BalanceEntity>(message);
        entity.CurrentState = CurrentStateConstants.PENDING;

        var saveEntity = await ProcessDatabase(entity);

        if (saveEntity.Id != 0)
        {
            var eventModel = new
            {
                saveEntity.CorrelationId,
                saveEntity.CustomerId,
            };

            await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.BALANCE_CONFIRMED);
        }
        else
        {
            var eventModel = new
            {
                saveEntity.CorrelationId,
                saveEntity.CustomerId
            };

            await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.BALANCE_FAILED);
        }
    }

    private async Task TransferConfirmedBalance(string message)
    {
    }

    private async Task TransferFailedBalance(string message)
    {
    }

    public async Task<BalanceEntity> ProcessDatabase(BalanceEntity entity)
    {
        var existEntity =
            await _databaseService.Balance.FirstOrDefaultAsync(x => x.CorrelationId == entity.CorrelationId);

        if (existEntity == null)
        {
            entity.BalanceDate = DateTime.UtcNow;
            await _databaseService.Balance.AddAsync(entity);
            await _databaseService.SaveAsync();
            return entity;
        }
        else
        {
            existEntity.BalanceDate = DateTime.UtcNow;
            existEntity.CurrentState = entity.CurrentState;
            _databaseService.Balance.Update(existEntity);
            return existEntity;
        }
    }
}
