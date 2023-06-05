using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Joker.Client;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerService.Core.ApplicationServices;

internal class PullRecordCommandService : ICommandService<PullRecordCommand>
{
    private readonly IGameInterfaceJokerClient _client;
    private readonly IGameRecordUtensil _gameRecordUtensil;
    private readonly ILogger<PullRecordCommandService> _logger;
    private readonly ICommandService<ManualHandleRecordCommand> _manualService;
    private readonly ICommandService<ScheduleHandleRecordCommand> _scheduleService;

    public PullRecordCommandService(
        ILogger<PullRecordCommandService> logger,
        IGameInterfaceJokerClient client,
        IGameRecordUtensil gameRecordUtensil,
        ICommandService<ScheduleHandleRecordCommand> scheduleService,
        ICommandService<ManualHandleRecordCommand> manualService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _gameRecordUtensil = gameRecordUtensil ?? throw new ArgumentNullException(nameof(gameRecordUtensil));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        _manualService = manualService ?? throw new ArgumentNullException(nameof(manualService));
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
                    var record = await _gameRecordUtensil
                        .SettlementConvertRecordAsync(
                            gameRecord,
                            cancellationToken)
                        .ConfigureAwait(false);

                    if (command.Source == ScheduleFormSource.Schedule)
                    {
                        await _scheduleService
                            .ExecuteAsync(
                                command: new(Record: record),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        await _manualService
                            .ExecuteAsync(
                                command: new(Record: record),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "遊戲紀錄處理發生例外.({gameRecord})", gameRecord);
                }
            }
        }
    }
}
