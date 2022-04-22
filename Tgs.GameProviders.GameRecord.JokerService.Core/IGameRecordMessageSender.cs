namespace Tgs.GameProviders.GameRecord.JokerService.Core;

public interface IGameRecordMessageSender
{
    ValueTask SendGameRoundAsync(GameRoundData data);
}
