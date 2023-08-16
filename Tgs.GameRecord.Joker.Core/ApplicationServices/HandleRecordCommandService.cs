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

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var externalCreatedOn = command.Info.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = command.Info.ExternalClosedOn.ToDateTimeOffset();
        var bet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
        var validBet = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = command.Info.Settle.ToDecimal().GetValueOrDefault(0);
        var commission = 0.0m;
        var turnover = command.Info.Bet.ToDecimal().GetValueOrDefault(0);
        var winLoss = command.Info.WinLoss.ToDecimal().GetValueOrDefault(0);
        var status = GameRecordStatus.Closed;
        var clientStatus = command.Info.WinLoss > 0 ? GameRecordClientStatus.Win : GameRecordClientStatus.Loss;

        await _client
        .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Info.ExternalRoundId,
            externalGameId: command.Info.ExternalGameId,
            playerCode: command.Info.PlayerCode,
            status: status,
            clientStatus: clientStatus,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            bet: bet,
            validBet: validBet,
            settle: settle,
            commission: commission,
            turnover: turnover,
            playerWinLoss: winLoss,
            additionalData: command.Info.AdditionalData,
            cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    }
}
