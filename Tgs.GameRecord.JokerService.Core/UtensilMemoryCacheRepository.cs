using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameCommand.Common;
using Tgs.GameProviders.Domains.SettingConfiguration;

namespace Tgs.GameRecord.JokerService.Core;

internal class UtensilMemoryCacheRepository : IUtensil
{
    private readonly TimeSpan _absoluteExpirationRelativeToNow;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IMemoryCache _memoryCache;
    private readonly IUtensil _utensil;

    public UtensilMemoryCacheRepository(
        IOptions<GameRecordInfoOptions> options,
        IUtensil utensil,
        IMemoryCache memoryCache)
    {
        _absoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        _gameRecordInfo = options.Value;
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    public async ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(
        Guid brandId,
        Guid gameProviderId,
        CancellationToken cancellationToken = default)
    {
        var key = $"{_gameRecordInfo.GameProviderCode}:{brandId}:{gameProviderId}";
        if (!_memoryCache.TryGetValue(key, out Guid? gameProviderConfigurationId))
        {
            gameProviderConfigurationId = await _utensil
                .BrandGetGameProviderConfigurationIdAsync(
                    brandId: brandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (gameProviderConfigurationId == null)
            {
                return null;
            }
            _ = _memoryCache.Set(key, gameProviderConfigurationId, absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
        }
        return gameProviderConfigurationId;
    }

    public async ValueTask<int?> GetGameIdAsync(
        Guid gameProviderId,
        string gameExternalId,
        CancellationToken cancellationToken = default)
    {
        var key = $"{_gameRecordInfo.GameProviderCode}:{gameExternalId}";
        if (!_memoryCache.TryGetValue(key, out int? gameId))
        {
            gameId = await _utensil
                .GetGameIdAsync(
                    gameProviderId: gameProviderId,
                    gameExternalId: gameExternalId,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (gameId.HasValue)
            {
                _ = _memoryCache.Set(key, gameId, absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
            }
        }
        return gameId;
    }

    public async ValueTask<Guid> GetGameProviderIdAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_memoryCache.TryGetValue(_gameRecordInfo.GameProviderCode, out Guid? gameProviderId))
        {
            gameProviderId = await _utensil
                .GetGameProviderIdAsync(
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            _ = _memoryCache.Set(_gameRecordInfo.GameProviderCode, gameProviderId);
        }
        return gameProviderId!.Value;
    }

    public async ValueTask<PlayerInfo?> GetPlayerAsync(
        string playerCode,
        CancellationToken cancellationToken = default)
    {
        var key = $"{_gameRecordInfo.GameProviderCode}:{playerCode}";
        if (!_memoryCache.TryGetValue(key, out PlayerInfo? playerModel))
        {
            playerModel = await _utensil
                .GetPlayerAsync(
                    playerCode,
                    cancellationToken)
                .ConfigureAwait(false);

            if (playerModel == null)
            {
                return null;
            }
            _ = _memoryCache.Set(key, playerModel, TimeSpan.FromDays(1));
        }
        return playerModel;
    }

    public ValueTask<JokerConfiguration> GetSettingConfigurationAsync(
        CancellationToken cancellationToken = default)
        => _utensil.GetSettingConfigurationAsync(
            cancellationToken);
}
