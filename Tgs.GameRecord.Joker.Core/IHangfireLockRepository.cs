namespace Tgs.GameRecord.Joker.Core;

public interface IHangfireLockRepository
{
	public Task<bool> LockingAsync(string application, CancellationToken cancellationToken = default);
}
