namespace Tgs.GameRecord.JokerService.ViewModels;

public record PullRecordViewModel(
    Uri ApiUrl,
    string AppId,
    string SecretKey,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
