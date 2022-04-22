namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public interface IPlayerService
{
    ValueTask<PlayerInfo?> GetAsync(Guid gameProviderId, string playerCode);
}
