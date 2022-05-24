using Tgs.GameProviders.Domains.SettingConfiguration;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.JokerService.Core
{
    public interface IUtensil
    {
        ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId);

        ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);

        ValueTask<Guid> GetGameProviderIdAsync();

        ValueTask<PlayerModel?> GetPlayerAsync(string playerCode);

        ValueTask<JokerConfiguration> GetSettingConfigurationAsync();
    }
}
