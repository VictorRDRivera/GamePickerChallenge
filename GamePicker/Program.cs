using GamePicker.Application;
using GamePicker.Application.Common.External;
using GamePicker.Application.Mapping;
using GamePicker.Infastructure;
using GamePicker.Middleware;
using GamePicker.Filters;
using MongoDB.Driver;
using RestSharp;
using RestSharp.Serializers.Json;
using System.Text.Json;
using GamePicker.Application.Common.Interfaces;
using GamePicker.Application.Common.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
{
    var mongoConn = builder.Configuration["MONGO_CONNECTION_STRING"] ?? "mongodb://localhost:27018";
    var redisConn = builder.Configuration["REDIS_CONNECTION_STRING"] ?? "localhost:6379";
    var apiBaseUrl = builder.Configuration["Proxies:FREE_GAMES_API_URL"] ?? "https://www.freetogame.com/";
    
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));
    
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConn;
        options.InstanceName = "GamePicker_";
    });
    
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        return ConnectionMultiplexer.Connect(redisConn);
    });
    
    builder.Services
        .AddApplication()
        .AddPersistence();

    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ApiResponseFilter>();
    });
    builder.Services.AddScoped<IFreeToPlayGamesClient, FreeToGameClient>();
    builder.Services.AddScoped<ICacheService, RedisCacheService>();
    builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Game Picker API",
            Version = "v1",
            Description = "RESTful API for suggesting games based on filters like genre, platform, and RAM.",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Victor Rivera",
                Email = "victor.rivera-@hotmail.com"
            }
        });
        
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    var snakeCaseOpts = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    builder.Services.AddSingleton<RestClient>(_ =>
    {
        var opts = new RestClientOptions(apiBaseUrl)
        {
            ThrowOnAnyError = false,
            Timeout = TimeSpan.FromSeconds(10)
        };

        return new RestClient(
            opts,
            configureSerialization: s => s.UseSystemTextJson(snakeCaseOpts));
    });
}

var app = builder.Build();
{
    using (var scope = app.Services.CreateScope())
    {
        var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
        var database = mongoClient.GetDatabase("gamepicker");
        var collection = database.GetCollection<GamePicker.Repository.Entities.GameRecommendationEntity>("GameRecommendations");
        var indexKeys = Builders<GamePicker.Repository.Entities.GameRecommendationEntity>.IndexKeys.Ascending(x => x.GameId);
        var indexModel = new CreateIndexModel<GamePicker.Repository.Entities.GameRecommendationEntity>(indexKeys, new CreateIndexOptions { Unique = true });
        try
        {
            collection.Indexes.CreateOne(indexModel);
        }
        catch (MongoCommandException ex) when (ex.Code == 85)
        {
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}