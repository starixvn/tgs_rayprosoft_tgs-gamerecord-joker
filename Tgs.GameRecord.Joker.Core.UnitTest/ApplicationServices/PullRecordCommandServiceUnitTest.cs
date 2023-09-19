using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.Joker.Client;
using Tgs.GameInterface.Joker.Protos;
using Tgs.GameRecord.Joker.Core.ApplicationServices;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.Joker.Core.UnitTest.ApplicationServices;

public class PullRecordCommandServiceUnitTest
{
    [Fact]
    public async Task Normal_Process()
    {
        var fakeLogger = NullLogger<PullRecordCommandService>.Instance;
        var fakeClient = Substitute.For<IGameInterfaceJokerClient>();
        var fakeScheduleService = Substitute.For<ICommandService<HandleRecordCommand>>();

        var command = new PullRecordCommand(
            ApiUrl: new("http://localhost/"),
            AppId: "appId",
            SecretKey: "secretKey",
            StartTime: DateTimeOffset.UtcNow.AddMinutes(-10),
            EndTime: DateTimeOffset.UtcNow,
            Source: ScheduleFormSource.Schedule);

        var reply = new GameRecordReply
        {
            NextId = Guid.NewGuid().ToString("N")
        };

        _ = fakeClient
            .GameRecordAsync(
            apiUrl: Arg.Is(command.ApiUrl),
            appId: Arg.Is(command.AppId),
            secretKey: Arg.Is(command.SecretKey),
            startTime: Arg.Is(command.StartTime),
            endTime: Arg.Is(command.EndTime),
            nextId: Arg.Any<string>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(reply);

        var service = new PullRecordCommandService(
            fakeLogger,
            fakeClient,
            fakeScheduleService);

        await service.ExecuteAsync(command: command);
    }
}
