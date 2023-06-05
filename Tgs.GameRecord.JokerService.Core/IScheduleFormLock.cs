namespace Tgs.GameRecord.JokerService.Core;

public interface IScheduleFormLock
{
    public Task<bool> LockingAsync(long id);

    public Task<bool> UnlockAsync(long id);
}
