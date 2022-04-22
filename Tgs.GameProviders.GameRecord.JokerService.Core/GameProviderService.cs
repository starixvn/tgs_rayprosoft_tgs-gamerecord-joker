using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Tgs.GameProviders.QueryServer.Client;

namespace Tgs.GameProviders.GameRecord.JokerService.Core;

internal class GameProviderService : IGameProviderService
{
    private readonly IGameProvidersQueryServerClient _client;
    private readonly IMemoryCache _memoryCache;

    public GameProviderService(
        IMemoryCache memoryCache,
        IGameProvidersQueryServerClient client)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask<Guid> GetGameProviderIdAsync(string gameProviderCode)
    {
        if (_memoryCache.TryGetValue(gameProviderCode, out Guid? gameProviderId))
        {
            return gameProviderId!.Value;
        }

        gameProviderId = await _client.GameProviderGetGameProviderIdAsync(gameProviderCode)
            .ConfigureAwait(false);

        if (!gameProviderId.HasValue)
        {
            _ = _memoryCache.Set(gameProviderCode, gameProviderId, TimeSpan.FromHours(1));
        }

        return gameProviderId!.Value;
    }
}
