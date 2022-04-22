namespace Tgs.GameProviders.GameRecord.JokerService.ReceiveHandler;

public record JokerGameCommand(string GameProviderCode, DateTimeOffset SendDateTime);
