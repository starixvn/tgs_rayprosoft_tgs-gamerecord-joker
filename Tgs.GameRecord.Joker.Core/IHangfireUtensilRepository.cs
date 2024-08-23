using Tgs.GameRecord.Joker.Core.JobApplicationServices;

namespace Tgs.GameRecord.Joker.Core;

public interface IHangfireUtensilRepository
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
