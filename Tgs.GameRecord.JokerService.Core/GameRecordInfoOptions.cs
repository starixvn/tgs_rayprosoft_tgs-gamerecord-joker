namespace Tgs.GameRecord.JokerService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public decimal IntervalMinute { get; set; }
    public decimal IntervalMinuteMax { get; set; }
    public string MongoDefine { get; set; } = default!;

    public string MongoGlobPattern { get; set; } = default!;
}
