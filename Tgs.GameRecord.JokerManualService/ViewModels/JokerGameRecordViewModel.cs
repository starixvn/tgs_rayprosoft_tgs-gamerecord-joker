namespace Tgs.GameRecord.JokerManualService.ViewModels;

public record JokerGameRecordViewModel(
    string BrandCode,
    string AgentId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
