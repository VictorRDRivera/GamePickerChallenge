using GamePicker.Application;
using GamePicker.Application.Common.External;
using GamePicker.Application.Common.Interfaces;
using GamePicker.Application.Common.Services;
using GamePicker.Application.Mapping;
using GamePicker.Filters;
using GamePicker.Infastructure;
using GamePicker.Infastructure.Contexts;
using GamePicker.Middleware;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using RestSharp.Serializers.Json;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
{
    var redisConn = builder.Configuration["REDIS_CONNECTION_STRING"] ?? "localhost:6379";
    var apiBaseUrl = builder.Configuration["Proxies:FREE_GAMES_API_URL"] ?? "https://www.freetogame.com/";
    
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
        .AddPersistence(builder.Configuration);

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
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => await db.Database.MigrateAsync());
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