using System;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.UnitTests;

[TestFixture]
internal class JokerGameRoundHandleServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameProviderService = Substitute.For<IGameProviderService>();
        var fakePlayerService = Substitute.For<IPlayerService>();
        var fakeGameService = Substitute.For<IGameService>();
        var fakeGameRecordMessageSender = Substitute.For<IGameRecordMessageSender>();

        var gameRecord = new GameRecordInfo(
            GameProviderType.Joker,
            Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc"),
            "4uf6k014jp5x",
            RoundStatus.Closed,
            RoundClientStatus.Win,
            "83ztt1eowy7qy",
            "quofrdenycyyn",
            DateTimeOffset.UtcNow.AddSeconds(-5),
            DateTimeOffset.UtcNow,
            10m,
            10m,
            10m,
            null,
            10m,
            20m,
            null,
            string.Empty);

        var playerInfo = new PlayerInfo(
            Guid.Parse("9f1ecf19-38cd-474f-b0d2-93638dc07d21"),
            gameRecord.PlayerCode,
            "HKD",
            Guid.Parse("cf99c783-acdf-48cc-b1cc-08d99a806de9"),
            Guid.NewGuid());

        _ = fakeGameProviderService.GetGameProviderIdAsync(gameRecord.GameProviderCode)
            .Returns(gameRecord.GameProviderId);

        _ = fakePlayerService.GetAsync(gameRecord.GameProviderId, gameRecord.PlayerCode)
            .Returns(playerInfo);

        _ = fakeGameService.GetGameIdAsync(gameRecord.GameProviderId, gameRecord.ExternalGameId)
            .Returns(2600025);

        var query = new JokerGameRoundHandleQuery(gameRecord);

        var sut = new JokerGameRoundHandleService(
            fakeGameProviderService,
            fakeGameRecordMessageSender,
            fakeGameService,
            fakePlayerService);

        var actual = await sut.GetAsync(query);

        Assert.True(actual);

        _ = fakeGameRecordMessageSender
            .Received(1)
            .SendGameRoundAsync(
                Arg.Is<GameRoundData>(x =>
                    x.GameProviderCode == query.GameRecord.GameProviderCode
                    && x.GameProviderId == query.GameRecord.GameProviderId
                    && x.ExternalGameId == query.GameRecord.ExternalGameId
                    && x.ExternalRoundId == query.GameRecord.ExternalRoundId
                    && x.PlayerCode == query.GameRecord.PlayerCode));
    }
}
