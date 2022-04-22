namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public record JokerGameRecordResult(
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime);
