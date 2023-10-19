using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Joker.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var externalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
        var bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
        var commission = 0.0m;
        var winLoss = command.Info.WinLoss.ToDecimal().GetValueOrDefault(0);
        var wagerState = winLoss > 0 ? WagerStates.Win : WagerStates.Lose;

        return _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            playerCode: command.Info.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: ResettleStates.None,
            externalRoundId: command.Info.ExternalRoundId,
            externalWagerId: command.Info.ExternalRoundId,
            externalGameId: command.Info.ExternalGameId,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: DateTimeOffset.UnixEpoch,
            bet: bet,
            validBet: bet,
            settle: settle,
            commission: commission,
            turnover: bet,
            winLoss: winLoss,
            additionalData: command.Info.AdditionalData,
            cancellationToken: cancellationToken);
    }
}
