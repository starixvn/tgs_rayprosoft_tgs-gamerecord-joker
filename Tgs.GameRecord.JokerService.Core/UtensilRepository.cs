using Microsoft.Extensions.Options;
using Tgs.GameCommand.Client;
using Tgs.GameCommand.Common;
using Tgs.GameProviders.Domains.SettingConfiguration;

namespace Tgs.GameRecord.JokerService.Core;

internal class UtensilRepository : IUtensil
{
    private readonly IGameCommandClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public UtensilRepository(
        IGameCommandClient client,
        IOptions<GameRecordInfoOptions> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameRecordInfo = options.Value;
    }

    public ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
        => _client.BrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId);

    public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
    {
        var gameModel = await _client.GameGetAsync(gameProviderId, gameExternalId).ConfigureAwait(false);
        return gameModel?.GameId;
    }

    public async ValueTask<Guid> GetGameProviderIdAsync()
    {
        var gameProviderId = await _client.GameProviderGetGameProviderIdAsync(_gameRecordInfo.GameProviderCode).ConfigureAwait(false);
        return gameProviderId!.Value;
    }

    public ValueTask<PlayerInfo?> GetPlayerAsync(string playerCode)
        => _client.PlayerGetAsync(playerCode);

    public ValueTask<JokerConfiguration> GetSettingConfigurationAsync()
        => _client.SettingGetConfigurationAsync<JokerConfiguration>(_gameRecordInfo.GameProviderConfiguration)!;
}
