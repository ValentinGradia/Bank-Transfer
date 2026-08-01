using Bank.Transfer.WebAPI.Application.Database;
using Bank.Transfer.WebAPI.Application.External;
using Bank.Transfer.WebAPI.Domain.Constants;
using Bank.Transfer.WebAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bank.Transfer.WebAPI.Application.Features.Process;

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
            case ReceiveFromTopicConstants.TRANSFER_INITIATED:
                await TransferInitiated(message);
                break;
        }
    }

    private async Task TransferInitiated(string message)
    {
        var entity = JsonConvert.DeserializeObject<TransferEntity>(message);
        entity.CurrentState = CurrentStateConstants.PENDING;

        var saveEntity = await ProcessDatabase(entity);

        if (saveEntity.Id != 0)
        {
            var eventModel = new
            {
                saveEntity.CorrelationId,
                saveEntity.CustomerId,
            };

            //MS Transaction
            await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSFER_CONFIRMED);
        }
        else
        {
            var eventModel = new
            {
                saveEntity.CorrelationId,
                saveEntity.CustomerId
            };

            //MS Transaction
            await _serviceBusSenderService.Execute(eventModel, SendToTopicConstants.TRANSFER_FAILED);
        }
    }

    public async Task<TransferEntity> ProcessDatabase(TransferEntity entity)
    {
        var existEntity =
            await _databaseService.Transfer.FirstOrDefaultAsync(x => x.CorrelationId == entity.CorrelationId);

        if (existEntity == null)
        {
            entity.TransferDate = DateTime.UtcNow;
            await _databaseService.Transfer.AddAsync(entity);
            await _databaseService.SaveAsync();
            return entity;
        }
        else
        {
            existEntity.TransferDate = DateTime.UtcNow;
            existEntity.CurrentState = entity.CurrentState;
            _databaseService.Transfer.Update(existEntity);
            return existEntity;
        }
    }
}
