using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.Domains.SettingConfiguration;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;

internal class JokerWorkerCommandService : ICommandService<JokerWorkerCommand>
{
    private readonly IGameProvidersQueryServerClient _client;
    private readonly IGameProviderService _gameProviderService;
    private readonly IGetService<JokerGameRecordQuery, JokerGameRecordResult> _getService;
    private readonly ILogger<JokerWorkerCommandService> _logger;
    private readonly IJokerGameProviderScheduleService _scheduleService;

    public JokerWorkerCommandService(
        IGameProvidersQueryServerClient client,
        IJokerGameProviderScheduleService scheduleService,
        IGameProviderService gameProviderService,
        IGetService<JokerGameRecordQuery, JokerGameRecordResult> getService,
        ILogger<JokerWorkerCommandService> logger)

    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
        _gameProviderService = gameProviderService ?? throw new ArgumentNullException(nameof(gameProviderService));
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(JokerWorkerCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _client
            .SettingGetConfigurationAsync<JokerConfiguration>(GameProviderConfigurations.Joker)
            .ConfigureAwait(false);

        if (configuration == null)
        {
            _logger.LogError("Joker configuration is null");
            return;
        }

        var gameProviderId =
            await _gameProviderService.GetGameProviderIdAsync(GameProviderType.Joker).ConfigureAwait(false);

        foreach (var brandMerchant in configuration.BrandMerchants)
        {
            foreach (var agent in brandMerchant.Agents.Where(ag => ag.Status))
            {
                try
                {
                    var schedule = await _scheduleService
                                       .GetAsync(gameProviderId, brandMerchant.BrandCode,
                                           agent.AgentId, agent.CurrencyCode)
                                       .ConfigureAwait(false) ??
                                   await _scheduleService
                                       .InitAsync(gameProviderId, brandMerchant.BrandCode,
                                           agent.AgentId, agent.CurrencyCode)
                                       .ConfigureAwait(false);

                    var sysTime = DateTimeOffset.UtcNow;

                    while (schedule != null)
                    {
                        var config = _scheduleService.GetConfig(schedule);

                        if (config.EndTime >= sysTime)
                            break;

                        var query = new JokerGameRecordQuery(
                            brandMerchant.BrandCode,
                            agent.CurrencyCode,
                            config.StartTime,
                            config.EndTime);

                        var result = await _getService.GetAsync(query, cancellationToken).ConfigureAwait(false);

                        if (!result.Success)
                        {
                            break;
                        }

                        schedule = await _scheduleService
                            .UpdateAsync(schedule, result.LastGameRecordTime, sysTime)
                            .ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "發生例外");
                }
            }
        }
    }
}
