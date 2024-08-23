using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.Joker.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.Joker.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	TimeProvider timeProvider,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	private readonly double _intervalMinutes = options.Value.IntervalMinutesMax < options.Value.IntervalMinutes
		? options.Value.IntervalMinutesMax
		: options.Value.IntervalMinutesMinimum > options.Value.IntervalMinutes
		? options.Value.IntervalMinutesMinimum
		: options.Value.IntervalMinutes;

	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var currentTime = timeProvider.GetUtcNow();
		var endTime = DateTimeOffset.ParseExact(currentTime.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:00K", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
		var startTime = endTime.AddMinutes(_intervalMinutes * -1);

		var message = new JokerGameRecordMessage
		{
			ApiUrl = command.ApiUrl,
			AppId = command.AppId,
			SecretKey = command.SecretKey,
			StartTime = startTime,
			EndTime = endTime
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = JokerNatsConst.SchedulePublish,
				Data = new
				{
					message.ApiUrl,
					message.AppId,
					message.SecretKey,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u")
				},
				Message = "This message publishing handler."
			}));

		return messageSender.PublishAsync(
			subject: JokerNatsConst.SchedulePublish,
			data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)),
			cancellationToken: cancellationToken);
	}
}
