using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.ApplicationServices;

internal class JokerGameRoundHandleService : IGetService<JokerGameRoundHandleQuery, bool>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<JokerGameRoundHandleService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly string _subject;
    private readonly IUtensil _utensil;

    public JokerGameRoundHandleService(
        IOptions<GameRecordInfoOptions> options,
        IMessageSender messageSender,
        IUtensil utensil,
        ILogger<JokerGameRoundHandleService> logger)
    {
        _gameRecordInfo = options.Value;
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subject = $"tgs.gp.gamerecord.database.round.{_gameRecordInfo.GameProviderCode.ToLower()}";
    }

    public async ValueTask<bool> GetAsync(
        JokerGameRoundHandleQuery query,
        CancellationToken cancellationToken = default)
    {
        var success = true;
        var gameProviderId = await _utensil.GetGameProviderIdAsync().ConfigureAwait(false);

        var playerModel = await _utensil.GetPlayerAsync(query.GameRecord.PlayerCode).ConfigureAwait(false);
        var gameId = await _utensil.GetGameIdAsync(gameProviderId, query.GameRecord.ExternalGameId).ConfigureAwait(false);
        if (playerModel == null || !gameId.HasValue)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _utensil.BrandGetGameProviderConfigurationIdAsync(playerModel.BrandId, gameProviderId).ConfigureAwait(false);
            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        var messageRequest = new GameRoundMessageRequest()
        {
            GameProviderCode = query.GameRecord.GameProviderCode,
            GameProviderId = query.GameRecord.GameProviderId,
            ExternalRoundId = query.GameRecord.ExternalRoundId,
            PlayerCode = playerModel == null ? string.Empty : playerModel.PlayerCode,
            GameId = gameId ?? -1,
            ExternalGameId = query.GameRecord.ExternalGameId,
            CurrencyCode = playerModel == null ? string.Empty : playerModel.DefaultWalletCurrencyCode,
            Status = query.GameRecord.Status,
            GameProviderConfigurationId = gameProviderConfigurationId ?? Guid.Empty,
            WalletId = playerModel?.DefaultWalletId ?? Guid.Empty,
            BrandId = playerModel?.BrandId ?? Guid.Empty,
            ExternalCreatedOn = query.GameRecord.ExternalCreatedOn,
            ExternalClosedOn = query.GameRecord.ExternalClosedOn,
            BetAmount = query.GameRecord.BetAmount,
            ValidBetAmount = query.GameRecord.ValidBetAmount,
            PlayerWinLoss = query.GameRecord.PlayerWinLoss,
            Commission = query.GameRecord.Commission,
            Turnover = query.GameRecord.Turnover,
            SettleAmount = query.GameRecord.SettleAmount,
            ClientStatus = query.GameRecord.ClientStatus,
            AdditionalData = query.GameRecord.AdditionalData,
            Success = success
        };

        _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{request}", DateTime.Now.ToString("o"), _subject, messageRequest);

        await _messageSender.PublishAsync(
            _subject,
            messageRequest.ToBson(),
            cancellationToken).ConfigureAwait(false);

        return success;
    }
}
