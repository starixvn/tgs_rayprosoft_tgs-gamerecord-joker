namespace Tgs.GameRecord.Joker.Core.Models;

public record RecordInfo(
	bool Success,
	Guid GameProviderId,
	string GameProviderCode,
	string ExternalRoundId,
	int GameId,
	string ExternalGameId,
	string PlayerCode,
	Guid WalletId,
	string CurrencyCode,
	Guid BrandId,
	Guid GameProviderConfigurationId,
	DateTimeOffset ExternalCreatedOn,
	DateTimeOffset ExternalClosedOn,
	decimal Bet,
	decimal ValidBet,
	decimal Settle,
	decimal Commission,
	decimal Turnover,
	decimal WinLoss,
	string AdditionalData);
