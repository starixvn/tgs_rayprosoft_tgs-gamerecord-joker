using Tgs.GameRounds.Schedule.Protos;

namespace Tgs.GameRecord.Joker.Core.ApplicationServices;

public record PullRecordCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    ScheduleFormSource Source);
