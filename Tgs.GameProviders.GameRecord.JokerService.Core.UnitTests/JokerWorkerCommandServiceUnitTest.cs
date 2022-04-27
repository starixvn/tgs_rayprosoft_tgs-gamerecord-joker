using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using NUnit.Framework;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.Domains.SettingConfiguration;
using Tgs.GameProviders.Domains.SettingConfiguration.Joker;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.UnitTests;

[TestFixture]
internal class JokerWorkerCommandServiceUnitTest
{
    [Test]
    public async Task Normal_Process()
    {
        var fakeGameProvidersQueryServerClient = Substitute.For<IGameProvidersQueryServerClient>();
        var fakeGameProviderScheduleService = Substitute.For<IJokerGameProviderScheduleService>();
        var fakeGameProviderService = Substitute.For<IGameProviderService>();
        var fakeGameRecordQueryService = Substitute.For<IGetService<JokerGameRecordQuery, JokerGameRecordResult>>();

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

        var scheduleConfig = new JokerGameProviderScheduleConfig
        {
            IntervalMinuteMax = 60m,
            IntervalMinute = 20m,
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.Zero),
            StartTime = DateTimeOffset.UtcNow.AddMinutes(-20),
            EndTime = DateTimeOffset.UtcNow
        };

        var schedule = new GameProviderSchedule
        {
            Id = Guid.NewGuid(),
            BrandCode = brandMerchant.BrandCode,
            AgentId = agent.AgentId,
            CurrencyCode = agent.CurrencyCode,
            GameProviderId = gameProviderId,
            GameProviderCode = GameProviderType.Joker,
            CreatedOn = DateTimeOffset.UtcNow,
            UpdatedOn = DateTimeOffset.UtcNow,
            Config = JsonSerializer.Serialize(scheduleConfig)
        };

        var gameRecordResult = new JokerGameRecordResult(true, 10, 10, 0, DateTimeOffset.UtcNow);

        var gameRecordQuery = new JokerGameRecordQuery(
            brandMerchant.BrandCode,
            agent.AgentId,
            scheduleConfig.StartTime,
            scheduleConfig.EndTime);

        _ = fakeGameProvidersQueryServerClient
            .SettingGetConfigurationAsync<JokerConfiguration>(Arg.Is(GameProviderConfigurations.Joker))
            .Returns(configuration);

        _ = fakeGameProviderService.GetGameProviderIdAsync(Arg.Is(GameProviderType.Joker))
         .Returns(gameProviderId);

        _ = fakeGameProviderScheduleService
            .GetAsync(
                Arg.Is(gameProviderId),
                Arg.Is(brandMerchant.BrandCode),
                Arg.Is(agent.AgentId),
                Arg.Is(agent.CurrencyCode))
            .Returns(schedule);

        _ = fakeGameProviderScheduleService.GetConfig(Arg.Is(schedule))
            .Returns(scheduleConfig);

        _ = fakeGameRecordQueryService.GetAsync(Arg.Is(gameRecordQuery))
            .Returns(gameRecordResult);

        var sut = new JokerWorkerCommandService(
            fakeGameProvidersQueryServerClient,
            fakeGameProviderScheduleService,
            fakeGameProviderService,
            fakeGameRecordQueryService,
            NullLogger<JokerWorkerCommandService>.Instance);

        await sut.ExecuteAsync(new JokerWorkerCommand()).ConfigureAwait(false);

        _ = fakeGameProviderScheduleService
            .Received(1)
            .UpdateAsync(
            Arg.Any<GameProviderSchedule>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>());

        _ = fakeGameRecordQueryService
            .Received(1)
            .GetAsync(Arg.Is(gameRecordQuery));
    }
}
