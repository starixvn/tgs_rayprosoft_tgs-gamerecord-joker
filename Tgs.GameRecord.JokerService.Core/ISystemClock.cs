namespace Tgs.GameRecord.JokerService.Core
{
    public interface ISystemClock
    {
        DateTimeOffset CurrentTime { get; }
    }
}
