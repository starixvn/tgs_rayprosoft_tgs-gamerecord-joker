namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public interface IGameService
{
    ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);
}
