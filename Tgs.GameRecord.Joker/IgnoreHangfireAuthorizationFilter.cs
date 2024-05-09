using Hangfire.Dashboard;

namespace Tgs.GameRecord.Joker;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
