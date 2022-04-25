namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public interface IJokerGameProviderScheduleService
{
    ValueTask<GameProviderSchedule?> GetAsync(Guid gameProviderId, string brandCode, string agentId, string currencyCode);

    JokerGameProviderScheduleConfig GetConfig(GameProviderSchedule gameProviderSchedule);

    ValueTask<GameProviderSchedule> InitAsync(Guid gameProviderId, string brandCode, string agentId, string currencyCode);

    ValueTask<GameProviderSchedule?> UpdateAsync(
        GameProviderSchedule gameProviderSchedule,
        DateTimeOffset? lastGameRecordTime,
        DateTimeOffset sysTime);
}
