namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public interface IGameProviderService
{
    ValueTask<Guid> GetGameProviderIdAsync(string gameProviderCode);
}
