namespace Tgs.GameRecord.CRSports.Core;

public record class GameRecordInfoOptions
{
	public string GameProviderCode { get; set; } = default!;

	public string GameProviderConfiguration { get; init; } = default!;

	public string GameExternalId { get; set; } = default!;

	public string HangfirePrefix { get; set; } = "gamerecord-cr-hangfire:";

	public string HangfireLocking { get; set; } = "gamerecord-cr-locking:";

	public double IntervalMinutes { get; init; } = 10;

	public double IntervalMinutesMax { get; init; } = 1440;

	public double IntervalMinutesMinimum { get; init; } = 1;
}
