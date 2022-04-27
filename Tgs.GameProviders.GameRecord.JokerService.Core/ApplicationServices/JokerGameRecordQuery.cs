namespace Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;

public record JokerGameRecordQuery(
    string BrandCode,
    string AgentId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
