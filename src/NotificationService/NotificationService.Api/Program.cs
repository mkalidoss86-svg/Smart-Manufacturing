using NotificationService.Application.Services;
using NotificationService.Domain.Interfaces;
using NotificationService.Infrastructure.EventStore;
using NotificationService.Infrastructure.Hubs;
using NotificationService.Infrastructure.Publishers;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add structured logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddJsonConsole();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = builder.Configuration.GetValue<long?>("SignalR:MaxMessageSize") ?? 102400;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("SignalR:ClientTimeoutSeconds", 60));
    options.KeepAliveInterval = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("SignalR:KeepAliveSeconds", 30));
});

// Configure Redis backplane for horizontal scaling
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
    });
    
    builder.Services.AddSignalR()
        .AddStackExchangeRedis(redisConnection, options =>
        {
            options.Configuration.ChannelPrefix = StackExchange.Redis.RedisChannel.Literal("NotificationService");
        });
}
else
{
    // Use in-memory cache for development/testing
    builder.Services.AddDistributedMemoryCache();
}

// Configure CORS for SignalR clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSignalRClients",
        policy => policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Configure connection limits
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = builder.Configuration.GetValue<int>("ConnectionLimits:MaxConnections", 1000);
    options.Limits.MaxConcurrentUpgradedConnections = builder.Configuration.GetValue<int>("ConnectionLimits:MaxUpgradedConnections", 1000);
});

// Register application services
builder.Services.AddScoped<IInspectionNotificationService, InspectionNotificationService>();
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();
builder.Services.AddScoped<IEventStore, RedisEventStore>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSignalRClients");

app.MapControllers();
app.MapHub<InspectionHub>("/hubs/inspections");

// Map health endpoint
app.MapHealthChecks("/health");

app.Logger.LogInformation("VisionFlow Notification Service started");

app.Run();
