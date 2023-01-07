namespace Tgs.GameRecord.JokerService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public double IntervalMinutes { get; set; } = 15d;
    public double IntervalMinutesMax { get; set; } = 60d;
    public double IntervalMinutesMinimum { get; set; } = 11d;
}
