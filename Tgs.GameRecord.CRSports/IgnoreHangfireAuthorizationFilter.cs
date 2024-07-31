using Hangfire.Dashboard;

namespace Tgs.GameRecord.CRSports;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}
