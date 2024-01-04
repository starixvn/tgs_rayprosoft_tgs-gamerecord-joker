using Tgs.GameRecord.Joker.Core;

namespace Tgs.GameRecord.Joker;

public record RedisOptions
{
    public string ConnectionString { get; set; } = "localhost";
}
