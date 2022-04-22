using Microsoft.AspNetCore.Mvc;
using Tgs.GameProviders.GameRecord.JokerService.Core;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameProviders.GameRecord.JokerService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Controllers
{
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
        ///             "currencyCode": "HKD",
        ///             "startTime": "2021-11-30T16:00:00+00:00",
        ///             "endTime": "2021-12-31T16:00:00+00:00"
        ///         }
        /// </remarks>
        [HttpPost]
        public async ValueTask<JokerGameRecordHandleResult> GameRecordAsync(
            [FromServices] IGetService<JokerGameRecordQuery, JokerGameRecordResult> queryService,
            JokerGameRecordViewModel viewModel)
        {
            var result = await queryService
                .GetAsync(
                    new JokerGameRecordQuery(
                        viewModel.BrandCode,
                        viewModel.CurrencyCode,
                        viewModel.StartTime,
                        viewModel.EndTime))
                .ConfigureAwait(false);

            return new JokerGameRecordHandleResult(
                result.SuccessCount,
                result.FailureCount,
                result.LastGameRecordTime);
        }
    }
}
