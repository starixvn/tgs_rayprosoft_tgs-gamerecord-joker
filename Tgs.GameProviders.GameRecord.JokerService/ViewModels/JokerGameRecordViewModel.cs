namespace Tgs.GameProviders.GameRecord.JokerService.ViewModels;

public record JokerGameRecordViewModel(
    string BrandCode,
    string CurrencyCode,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
