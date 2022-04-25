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
            LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01"), TimeSpan.FromHours(+8)),
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

        _ = fakeGameProvidersQueryServerClient
            .SettingGetConfigurationAsync<JokerConfiguration>(GameProviderConfigurations.Joker)
            .Returns(configuration);

        _ = fakeGameProviderService.GetGameProviderIdAsync(GameProviderType.Joker)
         .Returns(gameProviderId);

        _ = fakeGameProviderScheduleService
            .GetAsync(gameProviderId, brandMerchant.BrandCode, agent.AgentId, agent.CurrencyCode)
            .Returns(schedule);

        _ = fakeGameProviderScheduleService.GetConfig(schedule)
            .Returns(scheduleConfig);

        _ = fakeGameRecordQueryService.GetAsync(new JokerGameRecordQuery(brandMerchant.BrandCode, agent.CurrencyCode,
                scheduleConfig.StartTime, scheduleConfig.EndTime))
            .Returns(gameRecordResult);

        var sut = new JokerWorkerCommandService(
            fakeGameProvidersQueryServerClient,
            fakeGameProviderScheduleService,
            fakeGameProviderService,
            fakeGameRecordQueryService,
            NullLogger<JokerWorkerCommandService>.Instance);

        await sut.ExecuteAsync(new JokerWorkerCommand()).ConfigureAwait(false);

        _ = fakeGameProviderScheduleService
            .Received(Quantity.AtLeastOne())
            .UpdateAsync(
            Arg.Any<GameProviderSchedule>(),
            Arg.Any<DateTimeOffset>(),
            Arg.Any<DateTimeOffset>());

        _ = fakeGameRecordQueryService
            .Received(Quantity.AtLeastOne())
            .GetAsync(new JokerGameRecordQuery(brandMerchant.BrandCode, agent.CurrencyCode,
                scheduleConfig.StartTime, scheduleConfig.EndTime));
    }
}
