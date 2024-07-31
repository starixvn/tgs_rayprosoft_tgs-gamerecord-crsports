using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.CRSports.Common;

public record class CRSportsGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://api.orb-6789.com/")]
	public Uri ApiUrl { get; init; } = default!;

	[Required]
	[DefaultValue(typeof(int), "3238")]
	public int AgentId { get; init; }

	[Required]
	[DefaultValue(typeof(string), "FnzrUTi5fcsdrws7")]
	public string SecretKey { get; init; } = default!;

	[Required]
	[DefaultValue(typeof(string), "RT706test1")]
	public string AgentAccount { get; init; } = default!;

	[Required]
	[DefaultValue(typeof(string), "aaa123")]
	public string AgentPassword { get; init; } = default!;

	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(string), "2024-07-26T16:30:00+08:00")]
	public DateTimeOffset StartTime { get; init; }

	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(string), "2024-07-26T17:00:00+08:00")]
	public DateTimeOffset EndTime { get; init; }
}
