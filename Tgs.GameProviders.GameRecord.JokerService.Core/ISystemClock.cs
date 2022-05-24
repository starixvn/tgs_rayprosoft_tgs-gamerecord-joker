namespace Tgs.GameProviders.GameRecord.JokerService.Core
{
    public interface ISystemClock
    {
        DateTimeOffset CurrentTime { get; }
    }
}
