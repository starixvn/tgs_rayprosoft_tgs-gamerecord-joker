using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.Joker.Core.ApplicationServices;
using Tgs.GameRecord.Joker.ViewModels;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Joker.Controllers;

[Route("[controller]")]
[ApiController]
public class JokerController : ControllerBase
{
    /// <summary>
    /// Executions the asynchronous.
    /// </summary>
    /// <param name="service">The service.</param>
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
    [HttpPost]
    public ValueTask ExecutionAsync(
        [FromServices] ICommandService<PullRecordCommand> service,
        [FromBody] PullRecordViewModel source)
        => service
        .ExecuteAsync(
            command: new PullRecordCommand(
                source.ApiUrl,
                source.AppId,
                source.SecretKey,
                source.StartTime,
                source.EndTime,
                ScheduleFormSource.Manual),
            cancellationToken: HttpContext.RequestAborted);
}
