namespace Tgs.GameRecord.Joker.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; init; } = default!;

    public string GameProviderConfiguration { get; init; } = default!;

    public string HangfirePrefix { get; set; } = "gamerecord-joker-hangfire:";
    public string HangfireLocking { get; set; } = "gamerecord-joker-locking:";

    public double IntervalMinutes { get; init; } = 5;

    public double IntervalMinutesMax { get; init; } = 30;

    public double IntervalMinutesMinimum { get; init; } = 1;
}
