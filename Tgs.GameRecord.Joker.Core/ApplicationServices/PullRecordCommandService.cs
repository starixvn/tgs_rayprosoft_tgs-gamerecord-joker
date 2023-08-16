using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Joker.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Joker.Core.ApplicationServices;

internal class PullRecordCommandService : ICommandService<PullRecordCommand>
{
    private readonly IGameInterfaceJokerClient _client;
    private readonly ILogger<PullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public PullRecordCommandService(
        ILogger<PullRecordCommandService> logger,
        IGameInterfaceJokerClient client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        string nextId = string.Empty;
        while (true)
        {
            var reply = await _client
                .GameRecordAsync(
                apiUrl: command.ApiUrl,
                appId: command.AppId,
                secretKey: command.SecretKey,
                startTime: command.StartTime,
                endTime: command.EndTime,
                nextId: nextId,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            nextId = reply.NextId;

            if (!reply.Items.Any())
            {
                break;
            }

            foreach (var gameRecord in reply.Items)
            {
                try
                {
                    await _service
                        .ExecuteAsync(
                        command: new(Info: gameRecord),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameRecord);
                }
            }
        }
    }
}
