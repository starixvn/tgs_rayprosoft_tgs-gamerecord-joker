using System.Text;
using System.Text.Json;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.ReceiveHandler;

internal class JokerReceiveHandler : IMessageHandler
{
    private readonly ICommandService<JokerWorkerCommand> _commandService;
    private readonly ILogger<JokerReceiveHandler> _logger;

    public JokerReceiveHandler(
        ICommandService<JokerWorkerCommand> commandService,
        ILogger<JokerReceiveHandler> logger)
    {
        _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var command = JsonSerializer.Deserialize<JokerGameCommand>(json);

        var logInfo = new
        {
            GameProviderCode = GameProviderType.Joker,
            Data = command,
            ReceiveTime = DateTimeOffset.Now.ToString("O")
        };

        if (command == null)
        {
            _logger.LogError("Information Received:{LogInfo}", logInfo);
            return ValueTask.CompletedTask;
        }

        _logger.LogInformation("Information Received:{LogInfo}", logInfo);

        return _commandService.ExecuteAsync(new JokerWorkerCommand(), cancellationToken);
    }
}
