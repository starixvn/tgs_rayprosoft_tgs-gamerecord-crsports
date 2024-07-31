using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.CRSports.Common;

public record class CRSportsGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;

	[Required]
	public int AgentId { get; init; }

	[Required]
	public string SecretKey { get; init; } = default!;

	[Required]
	public string AgentAccount { get; init; } = default!;

	[Required]
	public string AgentPassword { get; init; } = default!;

	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; }

	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; }
}
