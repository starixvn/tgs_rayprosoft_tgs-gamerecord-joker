using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Joker.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Joker.Controllers;

[Route("[controller]")]
[ApiController]
public class JokerController : ControllerBase
{
	/// <summary>
	/// Games the record manual asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		JokerGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: JokerNatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		JokerGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: JokerNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}
