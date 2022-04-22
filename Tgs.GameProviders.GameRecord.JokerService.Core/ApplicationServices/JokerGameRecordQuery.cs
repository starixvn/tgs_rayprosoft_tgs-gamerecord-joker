namespace Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;

public record JokerGameRecordQuery(
    string BrandCode,
    string CurrencyCode,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
