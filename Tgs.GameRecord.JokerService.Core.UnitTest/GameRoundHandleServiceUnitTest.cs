using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameCommand.Common;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.JokerService.Core.UnitTest;

[TestFixture]
internal class GameRoundHandleServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions()
        {
            GameProviderCode = GameProviderType.Joker,
            GameProviderConfiguration = GameProviderConfigurations.Joker,
            IntervalMinutes = 15,
            IntervalMinutesMax = 60,
            IntervalMinutesMinimum = 11
        });
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var gameRecord = new GameRecordModle()
        {
            GameProviderCode = GameProviderType.Joker,
            GameProviderId = Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc"),
            PlayerCode = "4uf6k014jp5x",
            Status = GameRecordRoundStatus.Closed,
            ClientStatus = GameRecordRoundClientStatus.Win,
            ExternalRoundId = "83ztt1eowy7qy",
            ExternalGameId = "quofrdenycyyn",
            ExternalCreatedOn = DateTimeOffset.UtcNow.AddSeconds(-5),
            ExternalClosedOn = DateTimeOffset.UtcNow,
            BetAmount = 10,
            ValidBetAmount = 10,
            PlayerWinLoss = 10m,
            Commission = 0,
            Turnover = 10m,
            SettleAmount = 20m,
            TieBetAdjustment = null,
            AdditionalData = string.Empty
        };

        var playerInfo = new PlayerInfo(
            Guid.NewGuid(),
            "TGS",
            Guid.NewGuid(),
            "TGS",
            Guid.NewGuid(),
            "GPI",
            gameRecord.PlayerCode,
            gameRecord.PlayerCode,
            Guid.Parse("cf99c783-acdf-48cc-b1cc-08d99a806de9"),
            "HKD");

        _ = fakeUtensil.GetGameProviderIdAsync()
            .Returns(gameRecord.GameProviderId);

        _ = fakeUtensil.GetPlayerAsync(
            Arg.Is(gameRecord.PlayerCode))
            .Returns(playerInfo);

        var gameId = 2600025;
        _ = fakeUtensil.GetGameIdAsync(
                Arg.Is(gameRecord.GameProviderId),
                Arg.Is(gameRecord.ExternalGameId))
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil.GetBrandGameProviderConfigurationIdAsync(
            Arg.Is(playerInfo.BrandId),
            Arg.Is(gameRecord.GameProviderId)
            ).Returns(gameProviderConfigurationId);
        var query = new GameRoundHandleQuery(gameRecord);

        var sut = new GameRoundHandleService(
            fakeOptions,
            fakeClient,
            fakeUtensil);

        var actual = await sut.GetAsync(query);

        Assert.True(actual);

        await fakeClient.Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(gameRecord.GameProviderId),
                gameProviderCode: Arg.Is(fakeOptions.Value.GameProviderCode),
                externalRoundId: Arg.Is(query.GameRecord.ExternalRoundId),
                gameId: gameId,
                externalGameId: Arg.Is(query.GameRecord.ExternalGameId),
                playerCode: Arg.Is(playerInfo.PlayerCode),
                walletId: Arg.Is(playerInfo.DefaultWalletId),
                currencyCode: Arg.Is(playerInfo.DefaultWalletCurrencyCode),
                brandId: Arg.Is(playerInfo.BrandId),
                gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId),
                status: Arg.Is((RoundStatus)query.GameRecord.Status),
                clientStatus: Arg.Is((RoundClientStatus)query.GameRecord.ClientStatus),
                externalCreatedOn: Arg.Is(query.GameRecord.ExternalCreatedOn!.Value),
                externalClosedOn: Arg.Is(query.GameRecord.ExternalClosedOn),
                bet: Arg.Is(query.GameRecord.BetAmount.GetValueOrDefault(0)),
                validBet: Arg.Is(query.GameRecord.ValidBetAmount.GetValueOrDefault(0)),
                settle: Arg.Is(query.GameRecord.SettleAmount.GetValueOrDefault(0)),
                commission: Arg.Is(query.GameRecord.Commission.GetValueOrDefault(0)),
                turnover: Arg.Is(query.GameRecord.Turnover.GetValueOrDefault(0)),
                playerWinLoss: Arg.Is(query.GameRecord.PlayerWinLoss.GetValueOrDefault(0)),
                additionalData: Arg.Is(query.GameRecord.AdditionalData),
            Arg.Any<CancellationToken>()).ConfigureAwait(false);
    }

    [Test]
    public async Task Success_Is_False()
    {
        var fakeOptions = Options.Create(new GameRecordInfoOptions()
        {
            GameProviderCode = GameProviderType.Joker,
            GameProviderConfiguration = GameProviderConfigurations.Joker,
            IntervalMinutes = 15,
            IntervalMinutesMax = 60,
            IntervalMinutesMinimum = 11
        });
        var fakeClient = Substitute.For<IGameRoundsWorkClient>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var gameRecord = new GameRecordModle()
        {
            GameProviderCode = GameProviderType.Joker,
            GameProviderId = Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc"),
            PlayerCode = "4uf6k014jp5x",
            Status = GameRecordRoundStatus.Closed,
            ClientStatus = GameRecordRoundClientStatus.Win,
            ExternalRoundId = "83ztt1eowy7qy",
            ExternalGameId = "quofrdenycyyn",
            ExternalCreatedOn = DateTimeOffset.UtcNow.AddSeconds(-5),
            ExternalClosedOn = DateTimeOffset.UtcNow,
            BetAmount = 10,
            ValidBetAmount = 10,
            PlayerWinLoss = 10m,
            Commission = 0,
            Turnover = 10m,
            SettleAmount = 20m,
            TieBetAdjustment = null,
            AdditionalData = string.Empty
        };

        var playerInfo = new PlayerInfo(
            Guid.NewGuid(),
            "TGS",
            Guid.NewGuid(),
            "TGS",
            Guid.NewGuid(),
            "GPI",
            gameRecord.PlayerCode,
            gameRecord.PlayerCode,
            Guid.Parse("cf99c783-acdf-48cc-b1cc-08d99a806de9"),
            "HKD");

        _ = fakeUtensil.GetGameProviderIdAsync()
            .Returns(gameRecord.GameProviderId);

        _ = fakeUtensil.GetPlayerAsync(
            Arg.Is(gameRecord.PlayerCode))
            .Returns(playerInfo);

        var gameId = default(int?);
        _ = fakeUtensil.GetGameIdAsync(
                Arg.Is(gameRecord.GameProviderId),
                Arg.Is(gameRecord.ExternalGameId))
            .Returns(gameId);

        var gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil.GetBrandGameProviderConfigurationIdAsync(
            Arg.Is(playerInfo.BrandId),
            Arg.Is(gameRecord.GameProviderId)
            ).Returns(gameProviderConfigurationId);
        var query = new GameRoundHandleQuery(gameRecord);

        var sut = new GameRoundHandleService(
            fakeOptions,
            fakeClient,
            fakeUtensil);

        var actual = await sut.GetAsync(query);

        Assert.False(actual);

        await fakeClient
            .Received(1)
            .RoundMissing(
                gameProviderId: Arg.Is(gameRecord.GameProviderId),
                gameProviderCode: Arg.Is(fakeOptions.Value.GameProviderCode),
                externalRoundId: Arg.Is(query.GameRecord.ExternalRoundId),
                gameId: Arg.Is(-1),
                externalGameId: Arg.Is(query.GameRecord.ExternalGameId),
                playerCode: Arg.Is(playerInfo.PlayerCode),
                data: Arg.Any<string>(),
            Arg.Any<CancellationToken>()).ConfigureAwait(false);
    }
}
