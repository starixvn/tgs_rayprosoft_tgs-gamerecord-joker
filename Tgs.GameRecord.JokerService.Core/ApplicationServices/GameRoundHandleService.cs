using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerService.Core.ApplicationServices;

internal class GameRoundHandleService : IGetService<GameRoundHandleQuery, bool>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GameRoundHandleService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly MongoSettingOptions _mongoSetting;
    private readonly IUtensil _utensil;

    public GameRoundHandleService(
        ILogger<GameRoundHandleService> logger,
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IOptions<MongoSettingOptions> mongoSettingOptions,
        IMessageSender messageSender,
        IUtensil utensil
        )
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _mongoSetting = mongoSettingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    public async ValueTask<bool> GetAsync(
        GameRoundHandleQuery query,
        CancellationToken cancellationToken = default)
    {
        var success = true;
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var playerModel = await _utensil
            .GetPlayerAsync(
                playerCode: query.GameRecord.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                gameExternalId: query.GameRecord.ExternalGameId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (playerModel == null || !gameId.HasValue)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (playerModel != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                    brandId: playerModel.BrandId,
                    gameProviderId: gameProviderId,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            if (!gameProviderConfigurationId.HasValue)
            {
                success = false;
            }
        }

        var message = new GameRecordMessage(
            ExternalRoundId: query.GameRecord.ExternalRoundId,
            GameProviderId: gameProviderId,
            GameProviderCode: _gameRecordInfo.GameProviderCode,
            GameId: gameId ?? -1,
            ExternalGameId: query.GameRecord.ExternalGameId,
            PlayerCode: (playerModel == null) ? string.Empty : playerModel.PlayerCode,
            CurrencyCode: (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
            WalletId: (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
            BrandId: (playerModel == null) ? Guid.Empty : playerModel.BrandId,
            GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
            Status: (GameRoundStatus)query.GameRecord.Status,
            ClientStatus: (GameRoundClientStatus)query.GameRecord.ClientStatus,
            ExternalCreatedOn: query.GameRecord.ExternalCreatedOn!.Value,
            ExternalClosedOn: query.GameRecord.ExternalClosedOn,
            BetAmount: query.GameRecord.BetAmount.GetValueOrDefault(0),
            ValidBetAmount: query.GameRecord.BetAmount.GetValueOrDefault(0),
            SettleAmount: query.GameRecord.SettleAmount.GetValueOrDefault(0),
            Commission: query.GameRecord.Commission.GetValueOrDefault(0),
            Turnover: query.GameRecord.Turnover.GetValueOrDefault(0),
            PlayerWinLoss: query.GameRecord.PlayerWinLoss.GetValueOrDefault(0),
            AdditionalData: query.GameRecord.AdditionalData,
            Success: success);

        _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{message}",
            DateTime.Now.ToString("o"),
            _mongoSetting.GlobPattern,
            message);

        await _messageSender
            .PublishAsync(
                subject: _mongoSetting.GlobPattern,
                data: message.ToBson(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return success;
    }
}
