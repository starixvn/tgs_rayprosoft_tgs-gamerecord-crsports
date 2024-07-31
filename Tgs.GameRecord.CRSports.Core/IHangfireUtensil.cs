using Tgs.GameRecord.CRSports.Core.JobApplicationServices;

namespace Tgs.GameRecord.CRSports.Core;

public interface IHangfireUtensil
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}
