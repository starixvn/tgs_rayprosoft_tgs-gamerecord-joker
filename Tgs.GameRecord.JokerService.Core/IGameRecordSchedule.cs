using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.JokerService.Core;

public interface IGameRecordSchedule
{
    ValueTask<GameRecordScheduleReply?> GetAsync(string brandCode, string gameProviderAgentId, string currencyCode, CancellationToken cancellationToken = default);

    ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule);

    ValueTask<GameRecordScheduleReply?> InitAsync(Guid gameProviderId, string brandCode, string gameProviderAgentId, string currencyCode, CancellationToken cancellationToken = default);

    ValueTask<GameRecordScheduleReply?> UpdateAsync(GameRecordScheduleReply gameProviderSchedule, DateTimeOffset? lastGameRecordTime, DateTimeOffset sysTime, CancellationToken cancellationToken = default);
}
