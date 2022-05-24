namespace Tgs.GameProviders.GameRecord.JokerService.Core
{
    internal class SystemClock : ISystemClock
    {
        public DateTimeOffset CurrentTime => DateTimeOffset.UtcNow;
    }
}
