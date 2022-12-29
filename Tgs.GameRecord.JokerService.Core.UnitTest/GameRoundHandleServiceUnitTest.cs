using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameCommand.Common;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.MessageQueue;

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
            MongoDefine = "gp.gamerecord.database",
            MongoGlobPattern = "tgs.gp.gamerecord.database.round.joker",
            IntervalMinuteMax = 60,
            IntervalMinute = 20
        });
        var fakeMessageSender = Substitute.For<IMessageSender>();
        var fakeUtensil = Substitute.For<IUtensil>();
        var fakeLogger = NullLogger<GameRoundHandleService>.Instance;

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
        _ = fakeUtensil.BrandGetGameProviderConfigurationIdAsync(
            Arg.Is(playerInfo.BrandId),
            Arg.Is(gameRecord.GameProviderId)
            ).Returns(gameProviderConfigurationId);
        var query = new GameRoundHandleQuery(gameRecord);

        var sut = new GameRoundHandleService(
            fakeOptions,
            fakeMessageSender,
            fakeUtensil,
            fakeLogger);

        var actual = await sut.GetAsync(query);

        Assert.True(actual);

        await fakeMessageSender.Received(1)
            .PublishAsync(
            Arg.Is($"tgs.gp.gamerecord.database.round.joker"),
            Arg.Any<ReadOnlyMemory<byte>>()).ConfigureAwait(false);
    }
}
