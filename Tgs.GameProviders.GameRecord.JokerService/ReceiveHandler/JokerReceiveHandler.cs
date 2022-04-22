using Tgs.MessageQueue;

namespace Tgs.GameProviders.GameRecord.JokerService.ReceiveHandler;

internal class JokerReceiveHandler : IMessageHandler
{
    public ValueTask HandleAsync(
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
