using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Joker.Common;

public record JokerGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	public string AppId { get; init; } = default!;
	[Required]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset StartTime { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	public DateTimeOffset EndTime { get; init; } = default!;
}
