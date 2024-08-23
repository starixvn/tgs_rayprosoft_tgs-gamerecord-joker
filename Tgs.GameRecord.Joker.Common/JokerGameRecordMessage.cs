using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.Joker.Common;

public record JokerGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://w.api788.net/")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "THDT")]
	public string AppId { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "s5gawreoto5e1")]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-05-29T06:50:00+00:00")]
	public DateTimeOffset StartTime { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-05-29T07:00:00+00:00")]
	public DateTimeOffset EndTime { get; init; } = default!;
}
