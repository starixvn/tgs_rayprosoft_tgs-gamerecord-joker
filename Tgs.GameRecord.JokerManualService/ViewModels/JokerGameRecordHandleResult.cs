namespace Tgs.GameRecord.JokerManualService.ViewModels;

public record JokerGameRecordHandleResult(
    int SuccessCount,
    int FailureCount,
    DateTimeOffset? LastGameRecordTime);
