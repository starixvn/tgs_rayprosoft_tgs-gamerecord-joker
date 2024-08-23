using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Tgs.GameRecord.Handle.Client;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Joker.Core.ApplicationServices;

internal class HandleRecordCommandService(
	ILogger<HandleRecordCommandService> logger,
	IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
	IOptions<PolicySettingsOptions> policySettingsOptions,
	IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
	private readonly GameRecordInfoOptions _gameRecordInfoOptions = gameRecordInfoOptions.Value;
	private readonly PolicySettingsOptions _policySettingsOptions = policySettingsOptions.Value;

	public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
	{
		var externalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset();
		var externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
		var bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
		var settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
		var winLoss = command.Info.WinLoss.ToDecimal().GetValueOrDefault(0);
		var betState = winLoss > 0 ? Handle.Common.Protos.BetState.Win : Handle.Common.Protos.BetState.Loss;

		var source = new Handle.Common.ViewModels.GameRecordHandleViewModel
		{
			GameProviderCode = _gameRecordInfoOptions.GameProviderCode,
			PlayerCode = command.Info.PlayerCode,
			ExternalRoundId = command.Info.ExternalRoundId,
			ExternalBetId = command.Info.ExternalRoundId,
			ExternalGameId = command.Info.ExternalGameId,
			ExternalCreatedOn = externalCreatedOn,
			ExternalClosedOn = externalClosedOn,
			ExternalPayoffOn = externalClosedOn,
			ExternalResettleOn = DateTimeOffset.UnixEpoch,
			BetState = betState,
			BetType = Handle.Common.Protos.BetType.Normal,
			FreeGameState = Handle.Common.Protos.FreeGameState.None,
			GambleState = Handle.Common.Protos.GambleState.None,
			CashoutState = Handle.Common.Protos.CashoutState.None,
			ResettleState = Handle.Common.Protos.ResettleState.None,
			JackpotState = Handle.Common.Protos.JackpotState.None,
			Bet = bet,
			ValidBet = bet,
			Settle = settle,
			Commission = 0.0m,
			Turnover = bet,
			WinLoss = winLoss,
			Jackpot = 0.0M,
			AdditionalData = command.Info.AdditionalData
		};

		var retryCount = _policySettingsOptions.RetryCount;
		var retrySeconds = _policySettingsOptions.RetrySeconds;
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.WaitAndRetryAsync(
				retryCount: retryCount,
				retryAttempt => TimeSpan.FromSeconds(Math.Pow(retrySeconds, retryAttempt)),
				onRetry: (exception, retryTime, retryCount, context) => logger.LogWarning(exception, "{logWarning}",
					new
					{
						LogOn = DateTime.Now.ToString("u"),
						RetryTime = retryTime,
						RetryCount = retryCount,
						Data = new
						{
							source.GameProviderCode,
							source.PlayerCode,
							source.ExternalRoundId,
							source.ExternalBetId,
							source.ExternalGameId,
							ExternalCreatedOn = source.ExternalCreatedOn.ToString("u"),
							ExternalClosedOn = source.ExternalClosedOn.ToString("u"),
							source.BetState
						},
						exception.Message
					}));

		await retryPolicy.ExecuteAsync(() => client.HandleAsync(
			source: source,
			cancellationToken: cancellationToken).AsTask())
			.ConfigureAwait(false);
	}
}
