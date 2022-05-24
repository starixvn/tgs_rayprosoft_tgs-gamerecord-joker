namespace Tgs.GameRecord.JokerService.ViewModels;

public record JokerGameRecordViewModel(
    string BrandCode,
    string AgentId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
