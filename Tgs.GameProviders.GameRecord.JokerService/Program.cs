using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MongoDB.Messaging;
using Tgs.GameProviders.GameRecord.JokerService.Core.DependencyInjection;
using Tgs.GameProviders.GameRecord.JokerService.ReceiveHandler;
using Tgs.MessageQueue.MongoDB.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration
    .AddJsonFile("/config/appsettings.json", true)
    .AddEnvironmentVariables();

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .AddControllers()
    .AddJsonOptions(options
        => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddMemoryCache();

builder.Services
    .AddJokerGameRecordService(
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersBaseUrl");
        },
        (options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            options.BaseUrl = configuration.GetValue<Uri>("GameRecord:GameProvidersQueryServerBaseUrl");
        });

builder.Services
    .AddMessageQueue()
    .AddRabbitMessageQueue(config =>
    {
        _ = config.ConfigQueueOptions((options, sp) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            configuration.GetSection("RabbitMQ").Bind(options);

            options.OnSetupQueueAndExchange = channel =>
            {
                _ = channel.QueueDeclare(
                    "gp.gamerecord.joker",
                    true,
                    false,
                    false,
                    null);

                channel.QueueBind(
                    "gp.gamerecord.joker",
                    "amq.direct",
                    "gp.gamerecord.joker",
                    new Dictionary<string, object>());
            };
        }).AddHandler<JokerReceiveHandler>("gp.gamerecord.joker");
    })
    .AddMongoMessageQueue(config =>
    {
        _ = config
            .DefineMessageQueue(
                "gp.gamerecord.database",
                sp => new MongoMessageQueueBuilderOptions
                {
                    ConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("MongoDBMessagingConnectionString"),
                    MessageQueue = MessageQueue.Default
                });
    })
    .ClearAllExchanges()
    .AddMongoGlobPatternExchange(
        "tgs.gp.gamerecord.database.round.joker",
        "gp.gamerecord.database",
        "tgs.gp.gamerecord.database.round.joker");

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFileNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
    foreach (var xmlFileName in xmlFileNames)
    {
        options.IncludeXmlComments(xmlFileName);
    }

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Joker GameRecord Service",
        Version = "v1"
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/healthz");

app.Run();
