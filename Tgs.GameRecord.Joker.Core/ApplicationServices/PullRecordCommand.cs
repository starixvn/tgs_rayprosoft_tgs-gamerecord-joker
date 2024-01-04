namespace Tgs.GameRecord.Joker.Core.ApplicationServices;

public record PullRecordCommand(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
