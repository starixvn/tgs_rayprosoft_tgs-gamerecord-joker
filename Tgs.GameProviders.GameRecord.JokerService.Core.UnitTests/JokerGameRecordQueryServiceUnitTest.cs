using System;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.UnitTests;

[TestFixture]
internal class JokerGameRecordQueryServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameRecordClient = Substitute.For<IGameRecordListClient>();
        var fakeGameRoundQueryService = Substitute.For<IGetService<JokerGameRoundHandleQuery, bool>>();

        var externalClosedOn = DateTimeOffset.UtcNow;

        var gameRecordResponse = new JokerGameRecordResponse
        {
            Datas = new GameRecordModle[]
            {
                new GameRecordModle
                {
                    GameProviderCode = GameProviderType.Joker,
                    GameProviderId = Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc"),
                    PlayerCode = "4uf6k014jp5x",
                    BetAmount = 10m,
                    SettleAmount = 20m,
                    ValidBetAmount = 10m,
                    Turnover = 10m,
                    PlayerWinLoss = 10m,
                    ExternalRoundId = "83ztt1eowy7qy",
                    ExternalCreatedOn = externalClosedOn.AddSeconds(-5),
                    ExternalClosedOn = externalClosedOn,
                    ExternalGameId = "quofrdenycyyn",
                    Status = GameRecordRoundStatus.Closed,
                    ClientStatus = GameRecordRoundClientStatus.Win,
                    TieBetAdjustment = null,
                    Commission = null,
                    AdditionalData = string.Empty
                }
            },
            NextId = string.Empty,
            Success = true
        };

        var expected = new JokerGameRecordResult(
            true,
            1,
            1,
            0,
            externalClosedOn);

        var query = new JokerGameRecordQuery(
            "Default",
            "HKD",
            DateTimeOffset.UtcNow.AddMinutes(-20),
            DateTimeOffset.UtcNow);

        _ = fakeGameRecordClient.JokerGameRecordListAsync(
               Arg.Is(query.BrandCode),
               Arg.Is(query.CurrencyCode),
               Arg.Is(query.StartTime),
               Arg.Is(query.EndTime),
               Arg.Is((string?)null))
               .Returns(gameRecordResponse);

        _ = fakeGameRoundQueryService
            .GetAsync(Arg.Any<JokerGameRoundHandleQuery>())
            .Returns(true);

        var sut = new JokerGameRecordQueryService(fakeGameRecordClient, fakeGameRoundQueryService);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.AreEqual(expected, actual);

        _ = fakeGameRecordClient
            .Received(1)
            .JokerGameRecordListAsync(
                Arg.Is(query.BrandCode),
                Arg.Is(query.CurrencyCode),
                Arg.Is(query.StartTime),
                Arg.Is(query.EndTime),
                Arg.Any<string>());
    }

    [Test]
    public async Task Normal_Process_When_GetRecords_With_NextId()
    {
        var fakeGameRecordClient = Substitute.For<IGameRecordListClient>();
        var fakeGameRoundQueryService = Substitute.For<IGetService<JokerGameRoundHandleQuery, bool>>();

        var gameRecordResponseWithNextId = new JokerGameRecordResponse
        {
            Datas = new GameRecordModle[]
            {
                new GameRecordModle
                {
                    GameProviderCode = GameProviderType.Joker,
                    GameProviderId = Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc"),
                    PlayerCode = "4uf6k014jp5x",
                    BetAmount = 10m,
                    SettleAmount = 20m,
                    ValidBetAmount = 10m,
                    Turnover = 10m,
                    PlayerWinLoss = 10m,
                    ExternalRoundId = "83ztt1eowy7qy",
                    ExternalCreatedOn = DateTimeOffset.UtcNow.AddSeconds(-10),
                    ExternalClosedOn = DateTimeOffset.UtcNow.AddSeconds(-5),
                    ExternalGameId = "quofrdenycyyn",
                    Status = GameRecordRoundStatus.Closed,
                    ClientStatus = GameRecordRoundClientStatus.Win,
                    TieBetAdjustment = null,
                    Commission = null,
                    AdditionalData = string.Empty
                }
            },
            NextId = "HasNextId",
            Success = true
        };

        var gameRecordResponseWithoutNextId = new JokerGameRecordResponse
        {
            Datas = new GameRecordModle[]
            {
                new GameRecordModle
                {
                    GameProviderCode = GameProviderType.Joker,
                    GameProviderId = Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc"),
                    PlayerCode = "4uf6k014jp5x",
                    BetAmount = 10m,
                    SettleAmount = 20m,
                    ValidBetAmount = 10m,
                    Turnover = 10m,
                    PlayerWinLoss = 10m,
                    ExternalRoundId = "44d17p8u8nt8o",
                    ExternalCreatedOn = DateTimeOffset.UtcNow.AddSeconds(-5),
                    ExternalClosedOn = DateTimeOffset.UtcNow,
                    ExternalGameId = "quofrdenycyyn",
                    Status = GameRecordRoundStatus.Closed,
                    ClientStatus = GameRecordRoundClientStatus.Win,
                    TieBetAdjustment = null,
                    Commission = null,
                    AdditionalData = string.Empty
                }
            },
            NextId = string.Empty,
            Success = true
        };

        var expected = new JokerGameRecordResult(
            true,
            2,
            2,
            0,
            gameRecordResponseWithoutNextId.Datas.ElementAt(0).ExternalClosedOn);

        var query = new JokerGameRecordQuery(
            "Default",
            "HKD",
            DateTimeOffset.UtcNow.AddMinutes(-20),
            DateTimeOffset.UtcNow);

        _ = fakeGameRecordClient.JokerGameRecordListAsync(
            Arg.Is(query.BrandCode),
            Arg.Is(query.CurrencyCode),
            Arg.Is(query.StartTime),
            Arg.Is(query.EndTime),
            Arg.Is((string?)null))
               .Returns(gameRecordResponseWithNextId);

        _ = fakeGameRecordClient.JokerGameRecordListAsync(
            Arg.Is(query.BrandCode),
            Arg.Is(query.CurrencyCode),
            Arg.Is(query.StartTime),
            Arg.Is(query.EndTime),
            Arg.Is(gameRecordResponseWithNextId.NextId))
            .Returns(gameRecordResponseWithoutNextId);

        _ = fakeGameRoundQueryService
            .GetAsync(Arg.Any<JokerGameRoundHandleQuery>())
            .Returns(true);

        var sut = new JokerGameRecordQueryService(fakeGameRecordClient, fakeGameRoundQueryService);

        var actual = await sut.GetAsync(query).ConfigureAwait(false);

        Assert.AreEqual(expected, actual);

        _ = fakeGameRecordClient
            .Received(2)
            .JokerGameRecordListAsync(
                Arg.Is(query.BrandCode),
                Arg.Is(query.CurrencyCode),
                Arg.Is(query.StartTime),
                Arg.Is(query.EndTime),
                Arg.Any<string>());
    }
}
