using System.Text;
using System.Text.Json;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService
{
    public class ManualMessageHandler : IMessageProcessor
    {
        private readonly IGetService<JokerGameRecordQuery, JokerGameRecordResult> _service;

        public ManualMessageHandler(IGetService<JokerGameRecordQuery, JokerGameRecordResult> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var json = Encoding.UTF8.GetString(data.Span);
            var request = JsonSerializer.Deserialize<ManualMessageRequest>(json)!;

            var result = await _service.GetAsync(new JokerGameRecordQuery(
                BrandCode: request.BrandCode,
                AgentId: request.AgentId,
                StartTime: request.StartTime,
                EndTime: request.EndTime
                ), cancellationToken).ConfigureAwait(false);

            var response = new ManualMessageResponse()
            {
                Success = result.Success,
                LastGameRecordTime = result.LastGameRecordTime,
                SuccessCount = result.SuccessCount,
                FailCount = result.FailureCount,
                Count = result.Count
            };

            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
        }
    }
}
