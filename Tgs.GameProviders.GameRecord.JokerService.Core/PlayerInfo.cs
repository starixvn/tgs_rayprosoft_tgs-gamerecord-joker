namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public record PlayerInfo(
    Guid BrandId,
    string PlayerCode,
    string DefaultWalletCurrencyCode,
    Guid DefaultWalletId,
    Guid GameProviderConfigurationId);
