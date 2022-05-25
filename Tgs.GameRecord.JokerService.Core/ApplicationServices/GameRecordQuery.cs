namespace Tgs.GameRecord.JokerService.Core.ApplicationServices;

public record GameRecordQuery(
    string BrandCode,
    string AgentId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
