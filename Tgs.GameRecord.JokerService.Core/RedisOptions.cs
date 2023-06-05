namespace Tgs.GameRecord.JokerService.Core;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
