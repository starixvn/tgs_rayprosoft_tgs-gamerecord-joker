using MongoDB.Driver;

namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public interface IGameProviderScheduleRepository
{
    ValueTask<GameProviderSchedule?> GetAsync(Guid gameProviderId, string brandCode, string agentId, string currencyCode);

    ValueTask<UpdateResult> MergeAsync(GameProviderSchedule data);
}
