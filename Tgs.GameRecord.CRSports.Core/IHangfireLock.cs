namespace Tgs.GameRecord.CRSports.Core;

public interface IHangfireLock
{
	Task<bool> LockingAsync(string application, CancellationToken cancellationToken = default);
}
