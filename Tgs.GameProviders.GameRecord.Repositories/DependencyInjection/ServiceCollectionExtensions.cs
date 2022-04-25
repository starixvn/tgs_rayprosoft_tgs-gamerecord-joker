using Tgs.GameProviders.GameRecord.JokerService.Core;
using Tgs.GameProviders.GameRecord.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordMongoRepositories(
        this IServiceCollection services,
        Action<MongoDbOption, IServiceProvider> configure)
    {
        _ = services
            .AddOptions<MongoDbOption>()
            .Configure(configure);

        return services
            .AddTransient<IGameProviderScheduleRepository, GameProviderScheduleRepository>();
    }
}
