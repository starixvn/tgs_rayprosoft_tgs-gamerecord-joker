using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.JokerService.Core.ApplicationServices;

internal class GameRoundHandleService : IGetService<GameRoundHandleQuery, bool>
{
    private readonly IGameRoundsWorkClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IUtensil _utensil;

    public GameRoundHandleService(
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IGameRoundsWorkClient client,
        IUtensil utensil
        )
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
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

        if (success)
        {
            await _client
            .RoundBidding(
                gameProviderId: gameProviderId,
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                externalRoundId: query.GameRecord.ExternalRoundId,
                gameId: gameId!.Value,
                externalGameId: query.GameRecord.ExternalGameId,
                playerCode: playerModel!.PlayerCode,
                walletId: playerModel!.DefaultWalletId,
                currencyCode: playerModel!.DefaultWalletCurrencyCode,
                brandId: playerModel!.BrandId,
                gameProviderConfigurationId: gameProviderConfigurationId!.Value,
                status: (RoundStatus)query.GameRecord.Status,
                clientStatus: (RoundClientStatus)query.GameRecord.ClientStatus,
                externalCreatedOn: query.GameRecord.ExternalCreatedOn!.Value,
                externalClosedOn: query.GameRecord.ExternalClosedOn,
                bet: query.GameRecord.BetAmount.GetValueOrDefault(0),
                validBet: query.GameRecord.ValidBetAmount.GetValueOrDefault(0),
                settle: query.GameRecord.SettleAmount.GetValueOrDefault(0),
                commission: query.GameRecord.Commission.GetValueOrDefault(0),
                turnover: query.GameRecord.Turnover.GetValueOrDefault(0),
                playerWinLoss: query.GameRecord.PlayerWinLoss.GetValueOrDefault(0),
                additionalData: query.GameRecord.AdditionalData,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
        else
        {
            await _client
            .RoundMissing(
                gameProviderId: gameProviderId,
                gameProviderCode: _gameRecordInfo.GameProviderCode,
                externalRoundId: query.GameRecord.ExternalRoundId,
                gameId: gameId ?? -1,
                externalGameId: query.GameRecord.ExternalGameId,
                playerCode: playerModel?.PlayerCode ?? string.Empty,
                data: JsonSerializer.Serialize(query.GameRecord),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }

        return success;
    }
}
