using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;

internal class JokerGameRecordQueryService : IGetService<JokerGameRecordQuery, JokerGameRecordResult>
{
    private readonly IGameRecordListClient _gameRecordListClient;
    private readonly IGetService<JokerGameRoundHandleQuery, bool> _getService;

    public JokerGameRecordQueryService(
        IGameRecordListClient gameRecordListClient,
        IGetService<JokerGameRoundHandleQuery, bool> getService)
    {
        _gameRecordListClient = gameRecordListClient ?? throw new ArgumentNullException(nameof(gameRecordListClient));
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
    }

    public async ValueTask<JokerGameRecordResult> GetAsync(
        JokerGameRecordQuery query,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        var successCount = 0;
        var failureCount = 0;
        var lastRecordTime = default(DateTimeOffset?);

        var nextId = default(string?);

        do
        {
            var response = await _gameRecordListClient.JokerGameRecordListAsync(
                query.BrandCode,
                query.AgentId,
                query.StartTime,
                query.EndTime,
                nextId).ConfigureAwait(false);

            if (response == null || !response.Success)
            {
                break;
            }

            nextId = response.NextId;

            var records = response.Datas?.ToArray() ?? Array.Empty<GameRecordModle>();

            foreach (var data in records)
            {
                count++;

                var isSuccess = await _getService.GetAsync(
                        new JokerGameRoundHandleQuery(
                            new GameRecordInfo(
                                data.GameProviderCode,
                                data.GameProviderId,
                                data.PlayerCode,
                                (QueryServer.Models.RoundStatus)data.Status,
                                (QueryServer.Models.RoundClientStatus)data.ClientStatus,
                                data.ExternalRoundId,
                                data.ExternalGameId,
                                data.ExternalCreatedOn,
                                data.ExternalClosedOn,
                                data.BetAmount,
                                data.ValidBetAmount,
                                data.PlayerWinLoss,
                                data.Commission,
                                data.Turnover,
                                data.SettleAmount,
                                data.TieBetAdjustment,
                                data.AdditionalData)),
                        cancellationToken)
                    .ConfigureAwait(false);

                if (isSuccess)
                    successCount++;
                else
                    failureCount++;

                lastRecordTime = data.ExternalClosedOn ?? data.ExternalCreatedOn;
            }
        } while (!string.IsNullOrEmpty(nextId));

        return new JokerGameRecordResult(
            true,
            count,
            successCount,
            failureCount,
            lastRecordTime);
    }
}
