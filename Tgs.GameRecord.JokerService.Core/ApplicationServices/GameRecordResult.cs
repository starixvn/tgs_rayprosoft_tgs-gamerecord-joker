namespace Tgs.GameRecord.JokerService.Core.ApplicationServices;

public record GameRecordResult(
    bool Success,
    int Count,
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime);
