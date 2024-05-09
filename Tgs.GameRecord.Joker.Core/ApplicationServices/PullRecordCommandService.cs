using Microsoft.Extensions.Logging;
using Tgs.GameInterface.Joker.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Joker.Core.ApplicationServices;

internal class PullRecordCommandService(
	ILogger<PullRecordCommandService> logger,
	IGameInterfaceJokerClient client,
	ICommandService<HandleRecordCommand> service) : ICommandService<PullRecordCommand>
{
	public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
	{
		string nextId = string.Empty;
		while (true)
		{
			var reply = await client
				.GameRecordAsync(
				apiUrl: command.ApiUrl,
				appId: command.AppId,
				secretKey: command.SecretKey,
				startTime: command.StartTime,
				endTime: command.EndTime,
				nextId: nextId,
				cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			nextId = reply.NextId;

			if (reply.Items.Count == 0)
			{
				break;
			}

			foreach (var gameRecord in reply.Items)
			{
				try
				{
					await service
						.ExecuteAsync(
						command: new(Info: gameRecord),
						cancellationToken: cancellationToken)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameRecord);
				}
			}
		}
	}
}
