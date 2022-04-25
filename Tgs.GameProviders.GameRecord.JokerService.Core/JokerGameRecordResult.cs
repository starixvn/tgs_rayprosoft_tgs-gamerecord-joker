namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public record JokerGameRecordResult(
    bool Success,
    int Count,
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime);
