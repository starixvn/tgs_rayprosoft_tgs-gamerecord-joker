namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public record JokerGameProviderScheduleConfig
{
    public DateTimeOffset EndTime { get; set; }

    public decimal IntervalMinute { get; set; }

    public decimal IntervalMinuteMax { get; set; }

    public DateTimeOffset LastGameRecordTime { get; set; }

    public DateTimeOffset StartTime { get; set; }
}
