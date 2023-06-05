using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Tgs.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameRecord.JokerService.Core.Models;
using Tgs.GameRounds.Work.Manual.Client;
using Tgs.GameRounds.Work.Manual.Common.Protos;

namespace Tgs.GameRecord.JokerService.Core.UnitTest.ApplicationServices;

public class ManualHandleRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Settlement_Bets()
    {
        var fakeClient = Substitute.For<IGameRoundsWorkManualClient>();

        var command = new ManualHandleRecordCommand(
            Record: new RecordInfo(
                Success: true,
                GameProviderId: Guid.NewGuid(),
                GameProviderCode: "JOKER",
                ExternalRoundId: "OU1459290213398",
                GameId: 1,
                ExternalGameId: "test",
                PlayerCode: "4ucmqplxb029",
                WalletId: Guid.NewGuid(),
                CurrencyCode: "CNY",
                BrandId: Guid.NewGuid(),
                GameProviderConfigurationId: Guid.NewGuid(),
                ExternalCreatedOn: DateTimeOffset.Parse("2023-03-29T17:51:40.717+08:00"),
                ExternalClosedOn: DateTimeOffset.Parse("2023-03-29T17:51:40.717+08:00"),
                Bet: 10,
                ValidBet: 10,
                Settle: 20,
                Commission: 0,
                Turnover: 10,
                WinLoss: 10,
                AdditionalData: "{\"Id\":\"OU1459290213398\"}"
            ));

        var service = new ManualHandleRecordCommandService(
            fakeClient);

        await service
            .ExecuteAsync(
                command: command)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .RoundBidding(
                gameProviderId: Arg.Is(command.Record.GameProviderId),
                gameProviderCode: Arg.Is(command.Record.GameProviderCode),
                externalRoundId: Arg.Is(command.Record.ExternalRoundId),
                gameId: Arg.Is(command.Record.GameId),
                externalGameId: Arg.Is(command.Record.ExternalGameId),
                playerCode: Arg.Is(command.Record.PlayerCode),
                walletId: Arg.Is(command.Record.WalletId),
                currencyCode: Arg.Is(command.Record.CurrencyCode),
                brandId: Arg.Is(command.Record.BrandId),
                gameProviderConfigurationId: Arg.Is(command.Record.GameProviderConfigurationId),
                status: Arg.Is(RoundStatus.Closed),
                clientStatus: Arg.Is(RoundClientStatus.Win),
                externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn),
                externalClosedOn: Arg.Is(command.Record.ExternalClosedOn),
                bet: Arg.Is(command.Record.Bet),
                validBet: Arg.Is(command.Record.ValidBet),
                settle: Arg.Is(command.Record.Settle),
                commission: Arg.Is(command.Record.Commission),
                turnover: Arg.Is(command.Record.Turnover),
                playerWinLoss: Arg.Is(command.Record.WinLoss),
                additionalData: Arg.Is(command.Record.AdditionalData),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
