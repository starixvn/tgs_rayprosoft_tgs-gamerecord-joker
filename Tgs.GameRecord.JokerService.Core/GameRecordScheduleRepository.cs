using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameRecordSchedules.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.JokerService.Core;

internal class GameRecordScheduleRepository : IGameRecordSchedule
{
    private readonly IGameRecordSchedulesClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ISystemClock _systemClock;

    public GameRecordScheduleRepository(
        IOptions<GameRecordInfoOptions> options,
        IGameRecordSchedulesClient client,
        ISystemClock systemClock)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
    }

    public ValueTask<GameRecordScheduleReply?> GetAsync(string brandCode, string gameProviderAgentId, string currencyCode, CancellationToken cancellationToken = default)
    {
        var request = new GameRecordScheduleGetRequest()
        {
            GameProviderCode = _gameRecordInfo.GameProviderCode,
            BrandCode = brandCode,
            GameProviderAgentId = gameProviderAgentId,
            CurrencyCode = currencyCode
        };
        return _client.GameRecordScheduleGetAsync(request, cancellationToken);
    }

    public ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule)
        => JsonSerializer.Deserialize<ScheduleConfig>(gameProviderSchedule.Config)!;

    public async ValueTask<GameRecordScheduleReply?> InitAsync(Guid gameProviderId, string brandCode, string gameProviderAgentId, string currencyCode, CancellationToken cancellationToken = default)
    {
        var sysTime = _systemClock.CurrentTime;

        var config = new ScheduleConfig
        {
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.Zero),
            StartTime = sysTime.AddSeconds((double)_gameRecordInfo.IntervalMinute * 60 * -1),
            EndTime = sysTime
        };

        var addRequest = new GameRecordScheduleAddRequest()
        {
            GameProviderId = gameProviderId,
            GameProviderCode = _gameRecordInfo.GameProviderCode,
            BrandCode = brandCode,
            GameProviderAgentId = gameProviderAgentId,
            CurrencyCode = currencyCode,
            Config = JsonSerializer.Serialize(config)
        };

        _ = await _client.GameRecordScheduleAddAsync(addRequest, cancellationToken).ConfigureAwait(false);

        return await GetAsync(brandCode, gameProviderAgentId, currencyCode, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<GameRecordScheduleReply?> UpdateAsync(GameRecordScheduleReply gameProviderSchedule, DateTimeOffset? lastGameRecordTime, DateTimeOffset sysTime, CancellationToken cancellationToken = default)
    {
        var config = GetConfig(gameProviderSchedule);

        if (config.EndTime > sysTime || (sysTime - config.EndTime).TotalSeconds < (double)_gameRecordInfo.IntervalMinute * 60)
        {
            config.StartTime = sysTime.AddSeconds((double)_gameRecordInfo.IntervalMinute * 60 * -1);
            config.EndTime = sysTime;
        }
        else
        {
            var endTime = config.EndTime.AddSeconds((double)_gameRecordInfo.IntervalMinute * 60);
            config.StartTime = config.EndTime;
            config.EndTime = endTime > sysTime ? sysTime : endTime;
        }

        config.LastGameRecordTime = lastGameRecordTime ?? config.LastGameRecordTime;

        var request = new GameRecordScheduleUpdateRequest()
        {
            GameRecordScheduleId = gameProviderSchedule.GameRecordScheduleId,
            Config = JsonSerializer.Serialize(config)
        };

        var updateResult = await _client.GameRecordScheduleUpdateAsync(request, cancellationToken).ConfigureAwait(false);
        return updateResult.Success
            ? await GetAsync(gameProviderSchedule.BrandCode, gameProviderSchedule.GameProviderAgentId, gameProviderSchedule.CurrencyCode, cancellationToken).ConfigureAwait(false)
            : null;
    }
}
