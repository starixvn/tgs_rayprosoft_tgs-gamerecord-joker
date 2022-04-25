using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tgs.GameProviders.Client;
using Tgs.GameProviders.GameRecord.JokerService.Core.MessageSenders;
using Tgs.GameProviders.QueryServer.Client;

namespace Tgs.GameProviders.GameRecord.JokerService.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJokerGameRecordService(this IServiceCollection services,
        Action<GameProvidersClientOptions, IServiceProvider> gameProvidersClientOptions,
        Action<GameProvidersQueryServerClientOptions, IServiceProvider> gameProvidersQueryServerClientOptions)
        => services
            .RegisterAllServiceUtilityTypes(
                Assembly.GetExecutingAssembly(),
                ServiceLifetime.Transient)
            .AddGameProvidersClient(gameProvidersClientOptions)
            .AddGameProvidersQueryServerClient(gameProvidersQueryServerClientOptions)
            .AddTransient<IPlayerService, PlayerService>()
            .AddTransient<IGameProviderService, GameProviderService>()
            .AddTransient<IGameService, GameService>()
            .AddTransient<IJokerGameProviderScheduleService, JokerGameProviderScheduleService>()
            .AddTransient<IGameRecordMessageSender, GameRecordMessageSender>();
}
