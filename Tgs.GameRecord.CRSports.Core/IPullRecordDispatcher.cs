using Tgs.GameRecord.CRSports.Core.ApplicationServices;

namespace Tgs.GameRecord.CRSports.Core;

public interface IPullRecordDispatcher
{
	ValueTask PushCommandAsync(PullRecordCommand command, CancellationToken cancellationToken = default);
}
