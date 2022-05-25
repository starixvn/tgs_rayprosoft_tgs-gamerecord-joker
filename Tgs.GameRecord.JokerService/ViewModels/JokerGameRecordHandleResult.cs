namespace Tgs.GameRecord.JokerService.ViewModels;

public record JokerGameRecordHandleResult(
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime);
