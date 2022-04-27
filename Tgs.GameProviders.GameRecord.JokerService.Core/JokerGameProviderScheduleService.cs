using System.Text.Json;
using Tgs.GameProviders.Domains;

namespace Tgs.GameProviders.GameRecord.JokerService.Core;

internal class JokerGameProviderScheduleService : IJokerGameProviderScheduleService
{
    private readonly IGameProviderScheduleRepository _scheduleRepository;

    public JokerGameProviderScheduleService(
        IGameProviderScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
    }

    public ValueTask<GameProviderSchedule?> GetAsync(Guid gameProviderId, string brandCode, string agentId, string currencyCode)
        => _scheduleRepository.GetAsync(gameProviderId, brandCode, agentId, currencyCode);

    public JokerGameProviderScheduleConfig GetConfig(GameProviderSchedule gameProviderSchedule)
        => JsonSerializer.Deserialize<JokerGameProviderScheduleConfig>(gameProviderSchedule.Config)!;

    public async ValueTask<GameProviderSchedule> InitAsync(Guid gameProviderId, string brandCode, string agentId, string currencyCode)
    {
        var sysTime = DateTimeOffset.UtcNow;
        var intervalMinuteMax = 60m;
        var intervalMinute = 20m;
        var config = new JokerGameProviderScheduleConfig
        {
            IntervalMinuteMax = intervalMinuteMax,
            IntervalMinute = intervalMinute,
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.Zero),
            StartTime = sysTime.AddSeconds((double)intervalMinute * 60 * -1),
            EndTime = sysTime
        };

        var gameProviderSchedule = new GameProviderSchedule
        {
            Id = Guid.NewGuid(),
            GameProviderId = gameProviderId,
            GameProviderCode = GameProviderType.Joker,
            BrandCode = brandCode,
            AgentId = agentId,
            CurrencyCode = currencyCode,
            Config = JsonSerializer.Serialize(config),
            CreatedOn = sysTime,
            UpdatedOn = sysTime
        };

        _ = await _scheduleRepository.MergeAsync(gameProviderSchedule).ConfigureAwait(false);

        return gameProviderSchedule;
    }

    public async ValueTask<GameProviderSchedule?> UpdateAsync(
        GameProviderSchedule gameProviderSchedule,
        DateTimeOffset? lastGameRecordTime,
        DateTimeOffset sysTime)
    {
        var config = GetConfig(gameProviderSchedule);

        if (config.EndTime > sysTime || (sysTime - config.EndTime).TotalSeconds < (double)config.IntervalMinute * 60)
        {
            config.StartTime = sysTime.AddSeconds((double)config.IntervalMinute * 60 * -1);
            config.EndTime = sysTime;
        }
        else
        {
            var endTime = config.EndTime.AddSeconds((double)config.IntervalMinute * 60);
            config.StartTime = config.EndTime;
            config.EndTime = endTime > sysTime ? sysTime : endTime;
        }

        config.LastGameRecordTime = lastGameRecordTime ?? config.LastGameRecordTime;
        gameProviderSchedule.Config = JsonSerializer.Serialize(config);
        gameProviderSchedule.UpdatedOn = DateTimeOffset.UtcNow;

        var updateResult = await _scheduleRepository.MergeAsync(gameProviderSchedule).ConfigureAwait(false);

        return updateResult.ModifiedCount > 0
            ? await GetAsync(gameProviderSchedule.GameProviderId, gameProviderSchedule.BrandCode, gameProviderSchedule.AgentId, gameProviderSchedule.CurrencyCode).ConfigureAwait(false)
            : null;
    }
}
