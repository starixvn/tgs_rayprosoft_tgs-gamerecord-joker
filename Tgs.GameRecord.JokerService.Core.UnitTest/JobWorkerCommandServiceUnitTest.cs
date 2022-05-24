using System;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.Domains.SettingConfiguration;
using Tgs.GameProviders.Domains.SettingConfiguration.Joker;
using Tgs.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameRecord.JokerService.Core.JobWorkerServices;
using Tgs.GameRecordSchedules.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerService.Core.UnitTest;

[TestFixture]
internal class JobWorkerCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameRecordSchedule = Substitute.For<IGameRecordSchedule>();
        var fakeGameRecordQueryService = Substitute.For<IGetService<GameRecordQuery, GameRecordResult>>();
        var fakeLogger = NullLogger<JobWorkerCommandService>.Instance;
        var fakeSystemClock = Substitute.For<ISystemClock>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var currentTime = DateTimeOffset.UtcNow;
        _ = fakeSystemClock.CurrentTime.Returns(currentTime);
        var agent = new JokerAgent
        {
            AgentId = "TGVY",
            CurrencyCode = "HKD",
            SecretKey = "s5ghrn77baoto",
            Status = true
        };

        var brandMerchant = new JokerBrandMerchant
        {
            BrandCode = "Default",
            Agents = new[] { agent }
        };

        var configuration = new JokerConfiguration
        {
            ServiceEndpointUrl = new("http://game-joker"),
            ApiUrl = new("https://w.api788.net"),
            GameLaunchUrl = new Uri("http://www.gwp6868.net/"),
            BrandMerchants = new[] { brandMerchant }
        };

        var gameProviderId = Guid.Parse("2ebfe118-b36b-40fa-85b5-78363470addc");

        var scheduleConfig = new ScheduleConfig
        {
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.Zero),
            StartTime = currentTime.AddMinutes(-20),
            EndTime = currentTime.AddMinutes(-1)
        };

        var schedule = new GameRecordScheduleReply()
        {
            GameRecordScheduleId = Guid.NewGuid(),
            GameProviderId = gameProviderId,
            GameProviderCode = GameProviderType.Joker,
            BrandCode = brandMerchant.BrandCode,
            GameProviderAgentId = agent.AgentId,
            CurrencyCode = agent.CurrencyCode,
            Status = GameRecordScheduleStatus.Enable,
            Config = JsonSerializer.Serialize(scheduleConfig),
            CreatedOn = currentTime.ToTimestamp(),
            UpdatedOn = currentTime.ToTimestamp()
        };

        var gameRecordResult = new GameRecordResult(true, 10, 10, 0, currentTime);

        var gameRecordQuery = new GameRecordQuery(
            brandMerchant.BrandCode,
            agent.AgentId,
            scheduleConfig.StartTime,
            scheduleConfig.EndTime);

        _ = fakeUtensil
            .GetSettingConfigurationAsync()
            .Returns(configuration);

        _ = fakeUtensil.GetGameProviderIdAsync()
            .Returns(gameProviderId);

        _ = fakeGameRecordSchedule
            .GetAsync(
                Arg.Is(brandMerchant.BrandCode),
                Arg.Is(agent.AgentId),
                Arg.Is(agent.CurrencyCode))
            .Returns(schedule);

        _ = fakeGameRecordSchedule.GetConfig(Arg.Is(schedule))
            .Returns(scheduleConfig);

        _ = fakeGameRecordQueryService.GetAsync(Arg.Is(gameRecordQuery))
            .Returns(gameRecordResult);

        var sut = new JobWorkerCommandService(
         fakeGameRecordSchedule,
         fakeGameRecordQueryService,
         fakeLogger,
         fakeSystemClock,
         fakeUtensil);

        await sut.ExecuteAsync(new JobWorkerCommand()).ConfigureAwait(false);

        _ = fakeGameRecordSchedule
            .Received(1)
            .UpdateAsync(
            Arg.Any<GameRecordScheduleReply>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>());

        _ = fakeGameRecordQueryService
            .Received(1)
            .GetAsync(Arg.Is(gameRecordQuery));
    }
}
