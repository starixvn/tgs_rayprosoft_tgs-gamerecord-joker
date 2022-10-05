using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.JokerManualService.ViewModels;
using Tgs.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerManualService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JokerController : ControllerBase
{
    /// <summary>
    /// Games the record asynchronous.
    /// </summary>
    /// <param name="queryService">The service.</param>
    /// <param name="viewModel">The query.</param>
    /// <returns></returns>
    /// <remarks>
    /// <b>Example</b><br/>
    ///
    ///     Post
    ///         {
    ///             "brandCode": "Default",
    ///             "agentId": "TGVY",
    ///             "startTime": "2022-05-16T02:00:00+00:00",
    ///             "endTime": "2022-05-16T03:00:00+00:00"
    ///         }
    /// </remarks>
    [HttpPost]
    public async ValueTask<JokerGameRecordHandleResult> GameRecordAsync(
        [FromServices] IGetService<GameRecordQuery, GameRecordResult> queryService,
        JokerGameRecordViewModel viewModel)
    {
        var result = await queryService
            .GetAsync(
                new GameRecordQuery(
                    viewModel.BrandCode,
                    viewModel.AgentId,
                    viewModel.StartTime,
                    viewModel.EndTime))
            .ConfigureAwait(false);

        return new JokerGameRecordHandleResult(
            result.SuccessCount,
            result.FailureCount,
            result.LastGameRecordTime);
    }
}
