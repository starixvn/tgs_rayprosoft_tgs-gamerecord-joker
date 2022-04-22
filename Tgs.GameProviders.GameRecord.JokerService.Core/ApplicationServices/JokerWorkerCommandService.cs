using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Domains;
using Tgs.GameProviders.Domains.SettingConfiguration;
using Tgs.GameProviders.QueryServer.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;

internal class JokerWorkerCommandService : ICommandService<JokerWorkerCommand>
{
    private readonly IGameProvidersQueryServerClient _client;
    private readonly IGetService<JokerGameRecordQuery, JokerGameRecordResult> _getService;
    private readonly ILogger<JokerWorkerCommandService> _logger;

    public JokerWorkerCommandService(
        IGameProvidersQueryServerClient client,
        IGetService<JokerGameRecordQuery, JokerGameRecordResult> getService,
        ILogger<JokerWorkerCommandService> logger)

    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
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

        var timespan = TimeSpan.FromHours(8);
        var startTime = DateTimeOffset.UtcNow.AddMinutes(-20).ToOffset(timespan);
        var endTime = DateTimeOffset.UtcNow.ToOffset(timespan);

        foreach (var brandMerchant in configuration.BrandMerchants)
        {
            foreach (var agent in brandMerchant.Agents.Where(ag => ag.Status))
            {
                try
                {
                    var query = new JokerGameRecordQuery(
                        brandMerchant.BrandCode,
                        agent.CurrencyCode,
                        startTime,
                        endTime);

                    _ = await _getService.GetAsync(query, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "發生例外");
                }
            }
        }
    }
}
