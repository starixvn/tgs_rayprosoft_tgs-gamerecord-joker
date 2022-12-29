using Tgs.GameCommand.Common;
using Tgs.GameProviders.Domains.SettingConfiguration;

namespace Tgs.GameRecord.JokerService.Core;

public interface IUtensil
{
    ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId);

    ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId);

    ValueTask<Guid> GetGameProviderIdAsync();

    ValueTask<PlayerInfo?> GetPlayerAsync(string playerCode);

    ValueTask<JokerConfiguration> GetSettingConfigurationAsync();
}
