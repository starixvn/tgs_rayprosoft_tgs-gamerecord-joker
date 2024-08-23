using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.Joker.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Joker.Core.ApplicationServices;

namespace Tgs.GameRecord.Joker.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Settlement_Bets()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "JOKER",
			GameProviderConfiguration = "JOKERConfiguration",
			HangfirePrefix = "gamerecord-joker-hangfire:",
			HangfireLocking = "gamerecord-joker-locking:",
			IntervalMinutes = 5,
			IntervalMinutesMax = 30,
			IntervalMinutesMinimum = 1
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

		var command = new HandleRecordCommand(
			Info: new GameRecordInfo
			{
				ExternalRoundId = "OU1459290213398",
				ExternalGameId = "test",
				PlayerCode = "4ucmqplxb029",
				ExternalCreatedOn = DateTimeOffset.Parse("2023-03-29T17:51:40.717+08:00").ToTimestamp(),
				ExternalClosedOn = DateTimeOffset.Parse("2023-03-29T17:51:40.717+08:00").ToTimestamp(),
				Bet = 10,
				Settle = 20,
				WinLoss = 10,
				AdditionalData = "{\"Id\":\"OU1459290213398\"}"
			});

		var sut = new HandleRecordCommandService(
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await sut.ExecuteAsync(command: command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Info.PlayerCode,
				ExternalRoundId = command.Info.ExternalRoundId,
				ExternalBetId = command.Info.ExternalRoundId,
				ExternalGameId = command.Info.ExternalGameId,
				ExternalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Info.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 10.0m,
				ValidBet = 10.0m,
				Settle = 20.0m,
				Commission = 0.0M,
				Turnover = 10.0m,
				WinLoss = 10.0m,
				Jackpot = 0.0M,
				AdditionalData = command.Info.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
