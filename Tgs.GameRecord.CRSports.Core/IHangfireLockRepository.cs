namespace Tgs.GameRecord.CRSports.Core;

public interface IHangfireLockRepository
{
	Task<bool> LockingAsync(string application, CancellationToken cancellationToken = default);
}
