using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.Joker.Protos;
using Tgs.GameRecord.Joker.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.Joker.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Settlement_Bets()
    {
        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();

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

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameProviderCode = "365TF";
        _ = fakeUtensil
            .GetGameProviderCode()
            .Returns(gameProviderCode);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "PB2",
            PlayerCode: command.Info.PlayerCode,
            Username: "test",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "PHP"
            );
        _ = fakeUtensil
        .GetPlayerAsync(
                playerCode: Arg.Is(command.Info.PlayerCode),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        var gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
        gameProviderId: Arg.Is(gameProviderId),
                gameExternalId: Arg.Is(command.Info.ExternalGameId.ToString()),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
                brandId: Arg.Is(player.BrandId),
                gameProviderId: Arg.Is(gameProviderId),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var service = new HandleRecordCommandService(
            fakeUtensil,
            fakeClient);

        await service
            .ExecuteAsync(
                command: command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
            gameProviderId: Arg.Is(gameProviderId),
            gameProviderCode: Arg.Is(gameProviderCode),
            externalRoundId: Arg.Is(command.Info.ExternalRoundId),
            gameId: Arg.Is(gameId),
            externalGameId: Arg.Is(command.Info.ExternalGameId),
            playerCode: Arg.Is(player.PlayerCode),
            walletId: Arg.Is(player.DefaultWalletId),
            currencyCode: Arg.Is(player.DefaultWalletCurrencyCode),
            brandId: Arg.Is(player.BrandId),
            gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId),
            status: Arg.Is(RoundStatus.Closed),
            clientStatus: Arg.Is(RoundClientStatus.Win),
            externalCreatedOn: Arg.Is(command.Info.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.Info.ExternalClosedOn.ToDateTimeOffset()),
            bet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.Info.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(command.Info.Bet.ToDecimal().GetValueOrDefault(0)),
            playerWinLoss: Arg.Is(command.Info.WinLoss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Is(command.Info.AdditionalData),
            cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
