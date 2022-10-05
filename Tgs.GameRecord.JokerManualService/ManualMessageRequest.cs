namespace Tgs.GameRecord.JokerManualService;

public record ManualMessageRequest
{
    public string BrandCode { get; init; } = default!;

    public string AgentId { get; init; } = default!;

    public DateTimeOffset StartTime { get; init; } = default!;

    public DateTimeOffset EndTime { get; init; } = default!;
}
