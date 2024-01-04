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
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         {
    ///             "apiUrl": "https://w.api788.net/",
    ///             "appId": "THDT",
    ///             "secretKey": "s5gawreoto5e1",
    ///             "startTime": "2023-05-29T06:50:00+00:00",
    ///             "endTime": "2023-05-29T07:00:00+00:00"
    ///         }
    /// </remarks>
    [HttpPost("manual")]
    public ValueTask GameRecordManualAsync(
        [FromServices] IMessageSender messageSender,
        JokerGameRecordMessage source)
        => messageSender.PublishAsync(
            subject: JokerNatsConst.ManualPublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);

    /// <summary>
    /// Games the record schedule asynchronous.
    /// </summary>
    /// <param name="messageSender">The message sender.</param>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br />
    ///
    ///     Post
    ///
    ///         {
    ///             "apiUrl": "https://w.api788.net/",
    ///             "appId": "THDT",
    ///             "secretKey": "s5gawreoto5e1",
    ///             "startTime": "2023-05-29T06:50:00+00:00",
    ///             "endTime": "2023-05-29T07:00:00+00:00"
    ///         }
    /// </remarks>
    [HttpPost("schedule")]
    public ValueTask GameRecordScheduleAsync(
        [FromServices] IMessageSender messageSender,
        JokerGameRecordMessage source)
        => messageSender.PublishAsync(
            subject: JokerNatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(source)),
            cancellationToken: HttpContext.RequestAborted);
}
