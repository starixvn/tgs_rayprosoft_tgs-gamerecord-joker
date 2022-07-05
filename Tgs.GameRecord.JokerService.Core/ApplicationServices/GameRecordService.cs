using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerService.Core.ApplicationServices;

internal class GameRecordService : IGetService<GameRecordQuery, GameRecordResult>
{
    private readonly IGameRecordListClient _gameRecordListClient;
    private readonly IGetService<GameRoundHandleQuery, bool> _getService;

    public GameRecordService(
        IGameRecordListClient gameRecordListClient,
        IGetService<GameRoundHandleQuery, bool> getService)
    {
        _gameRecordListClient = gameRecordListClient ?? throw new ArgumentNullException(nameof(gameRecordListClient));
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
    }

    public async ValueTask<GameRecordResult> GetAsync(
        GameRecordQuery query,
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
                brandCode: query.BrandCode,
                agentId: query.AgentId,
                startTime: query.StartTime,
                endTime: query.EndTime,
                nextId: nextId,
                cancellationToken: cancellationToken).ConfigureAwait(false);

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
                        new GameRoundHandleQuery(data),
                        cancellationToken)
                    .ConfigureAwait(false);

                if (isSuccess)
                    successCount++;
                else
                    failureCount++;

                lastRecordTime = data.ExternalClosedOn ?? data.ExternalCreatedOn;
            }
        } while (!string.IsNullOrEmpty(nextId));

        return new GameRecordResult(
            true,
            count,
            successCount,
            failureCount,
            lastRecordTime);
    }
}
