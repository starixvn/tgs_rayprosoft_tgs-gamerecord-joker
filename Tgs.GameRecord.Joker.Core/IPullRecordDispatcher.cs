using Tgs.GameRecord.Joker.Core.ApplicationServices;

namespace Tgs.GameRecord.Joker.Core;

public interface IPullRecordDispatcher
{
	ValueTask PushCommandAsync(PullRecordCommand command, CancellationToken cancellationToken = default);
}
