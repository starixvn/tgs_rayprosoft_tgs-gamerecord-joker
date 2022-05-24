using Microsoft.Extensions.Logging;
using Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.JobWorkerServices;

internal class JokerWorkerCommandService : ICommandService<JokerWorkerCommand>
{
    private readonly IGameRecordSchedule _gameRecordSchedule;
    private readonly IGetService<JokerGameRecordQuery, JokerGameRecordResult> _getService;
    private readonly ILogger<JokerWorkerCommandService> _logger;
    private readonly ISystemClock _systemClock;
    private readonly IUtensil _utensil;

    public JokerWorkerCommandService(
        IGameRecordSchedule gameRecordSchedule,
        IGetService<JokerGameRecordQuery, JokerGameRecordResult> getService,
        ILogger<JokerWorkerCommandService> logger,
        ISystemClock systemClock,
        IUtensil utensil)

    {
        _gameRecordSchedule = gameRecordSchedule ?? throw new ArgumentNullException(nameof(gameRecordSchedule));
        _getService = getService ?? throw new ArgumentNullException(nameof(getService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask ExecuteAsync(JokerWorkerCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _utensil.GetSettingConfigurationAsync().ConfigureAwait(false);

        var gameProviderId = await _utensil.GetGameProviderIdAsync().ConfigureAwait(false);

        if (configuration == null)
        {
            _logger.LogError("Joker configuration is null");
            return;
        }

        foreach (var brandMerchant in configuration.BrandMerchants)
        {
            foreach (var agent in brandMerchant.Agents.Where(ag => ag.Status))
            {
                try
                {
                    var schedule = await _gameRecordSchedule
                                       .GetAsync(
                                           brandMerchant.BrandCode,
                                           agent.AgentId,
                                           agent.CurrencyCode,
                                           cancellationToken)
                                       .ConfigureAwait(false) ??
                                   await _gameRecordSchedule
                                       .InitAsync(
                                            gameProviderId,
                                            brandMerchant.BrandCode,
                                            agent.AgentId,
                                            agent.CurrencyCode,
                                            cancellationToken)
                                       .ConfigureAwait(false);

                    var sysTime = _systemClock.CurrentTime;

                    while (schedule != null)
                    {
                        var config = _gameRecordSchedule.GetConfig(schedule);

                        if (config.EndTime >= sysTime)
                            break;

                        var query = new JokerGameRecordQuery(
                            brandMerchant.BrandCode,
                            agent.AgentId,
                            config.StartTime,
                            config.EndTime);

                        var result = await _getService.GetAsync(query, cancellationToken).ConfigureAwait(false);

                        if (result.Success)
                            schedule = await _gameRecordSchedule
                                .UpdateAsync(
                                schedule,
                                result.LastGameRecordTime,
                                sysTime,
                                cancellationToken).ConfigureAwait(false);
                        else
                            break;
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
