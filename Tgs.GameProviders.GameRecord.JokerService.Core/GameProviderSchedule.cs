using MongoDB.Bson.Serialization.Attributes;

namespace Tgs.GameProviders.GameRecord.JokerService.Core
{
    public class GameProviderSchedule
    {
        [BsonElement("agent_id")]
        public string AgentId { get; init; } = default!;

        [BsonElement("brand_code")]
        public string BrandCode { get; init; } = default!;

        [BsonElement("config")]
        public string Config { get; set; } = default!;

        [BsonElement("created_on")]
        public DateTimeOffset CreatedOn { get; set; }

        [BsonElement("currency_code")]
        public string CurrencyCode { get; set; } = default!;

        [BsonElement("game_provider_code")]
        public string GameProviderCode { get; init; } = default!;

        [BsonElement("game_provider_id")]
        public Guid GameProviderId { get; init; }

        [BsonId]
        public Guid Id { get; set; }

        [BsonElement("updated_on")]
        public DateTimeOffset UpdatedOn { get; set; }
    }
}
