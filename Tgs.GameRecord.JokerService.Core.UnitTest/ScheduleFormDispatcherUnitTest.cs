using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tgs.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameRounds.Schedule.Client;
using Tgs.GameRounds.Schedule.Common.Protos;
using Tgs.GameRounds.Schedule.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerService.Core.UnitTest;

public class ScheduleFormDispatcherUnitTest
{
    [Fact]
    public async Task Exception_Occurs()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakePullRecord = Substitute.For<ICommandService<PullRecordCommand>>();

        var gameProviderCode = "JOKER";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new JokerParameter
        {
            ApiUrl = "http://localhost/",
            AppId = "appId",
            SecretKey = "secretKey",
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10).ToTimestamp(),
            EndTime = DateTimeOffset.UtcNow.ToTimestamp(),
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.joker.queued",
            Source = ScheduleFormSource.Schedule,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        var scheduleCommand = new PullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            AppId: parameter.AppId,
            SecretKey: parameter.SecretKey,
            StartTime: parameter.StartTime.ToDateTimeOffset(),
            EndTime: parameter.EndTime.ToDateTimeOffset(),
            Source: scheduleFormReply.Source);

        _ = fakePullRecord
           .ExecuteAsync(
               command: Arg.Is(scheduleCommand),
               cancellationToken: Arg.Any<CancellationToken>())
           .Throws<Exception>();

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakePullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormFaileAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                status: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task Normal_Process_Manual()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakePullRecord = Substitute.For<ICommandService<PullRecordCommand>>();

        var gameProviderCode = "JOKER";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new JokerParameter
        {
            ApiUrl = "http://localhost/",
            AppId = "appId",
            SecretKey = "secretKey",
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10).ToTimestamp(),
            EndTime = DateTimeOffset.UtcNow.ToTimestamp(),
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.joker.queued",
            Source = ScheduleFormSource.Manual,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakePullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        var scheduleCommand = new PullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            AppId: parameter.AppId,
            SecretKey: parameter.SecretKey,
            StartTime: parameter.StartTime.ToDateTimeOffset(),
            EndTime: parameter.EndTime.ToDateTimeOffset(),
            Source: scheduleFormReply.Source);

        await fakePullRecord
            .Received(1)
            .ExecuteAsync(
                command: Arg.Is(scheduleCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task Normal_Process_Schedule()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakePullRecord = Substitute.For<ICommandService<PullRecordCommand>>();

        var gameProviderCode = "JOKER";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new JokerParameter
        {
            ApiUrl = "http://localhost/",
            AppId = "appId",
            SecretKey = "secretKey",
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10).ToTimestamp(),
            EndTime = DateTimeOffset.UtcNow.ToTimestamp(),
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.joker.queued",
            Source = ScheduleFormSource.Schedule,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Queued,
            Status = $"{ScheduleFormState.Queued}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        _ = fakeScheduleFormLock
            .LockingAsync(
                id: Arg.Is(id))
            .Returns(true);

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakePullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        var scheduleCommand = new PullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            AppId: parameter.AppId,
            SecretKey: parameter.SecretKey,
            StartTime: parameter.StartTime.ToDateTimeOffset(),
            EndTime: parameter.EndTime.ToDateTimeOffset(),
            Source: scheduleFormReply.Source);

        await fakePullRecord
            .Received(1)
            .ExecuteAsync(
                command: Arg.Is(scheduleCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(1)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(1)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task Schedule_Form_State_Is_Not_Queued()
    {
        var fakeLogger = NullLogger<ScheduleFormDispatcher>.Instance;
        var fakeClient = Substitute.For<IGameRoundsScheduleClient>();
        var fakeScheduleFormLock = Substitute.For<IScheduleFormLock>();
        var fakePullRecord = Substitute.For<ICommandService<PullRecordCommand>>();

        var gameProviderCode = "JOKER";
        var id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var parameter = new JokerParameter
        {
            ApiUrl = "http://localhost/",
            AppId = "appId",
            SecretKey = "secretKey",
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-10).ToTimestamp(),
            EndTime = DateTimeOffset.UtcNow.ToTimestamp(),
        };
        var scheduleFormReply = new ScheduleFormReply
        {
            Id = id,
            GameProviderId = Guid.NewGuid(),
            GameProviderCode = gameProviderCode,
            Name = "test",
            Subject = "gp.gamerecord.scheduleform.joker.queued",
            Source = ScheduleFormSource.Schedule,
            Parameters = JsonSerializer.Serialize(parameter),
            State = ScheduleFormState.Processing,
            Status = $"{ScheduleFormState.Processing}..."
        };
        _ = fakeClient
            .ScheduleFormGetAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(scheduleFormReply);

        var dispatcher = new ScheduleFormDispatcher(
            fakeLogger,
            fakeClient,
            fakeScheduleFormLock,
            fakePullRecord);

        await dispatcher
            .ExecuteAsync(
                 gameProviderCode: gameProviderCode,
                id: id)
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .LockingAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);

        var scheduleCommand = new PullRecordCommand(
            ApiUrl: new Uri(parameter.ApiUrl),
            AppId: parameter.AppId,
            SecretKey: parameter.SecretKey,
            StartTime: parameter.StartTime.ToDateTimeOffset(),
            EndTime: parameter.EndTime.ToDateTimeOffset(),
            Source: scheduleFormReply.Source);

        await fakePullRecord
            .Received(0)
            .ExecuteAsync(
                command: Arg.Is(scheduleCommand),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .ScheduleFormProcessingAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        await fakeClient
            .Received(0)
            .ScheduleFormCompleteAsync(
                gameProviderCode: Arg.Is(gameProviderCode),
                id: Arg.Is(id),
                cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);

        _ = await fakeScheduleFormLock
            .Received(0)
            .UnlockAsync(
                id: Arg.Is(id))
            .ConfigureAwait(false);
    }
}
