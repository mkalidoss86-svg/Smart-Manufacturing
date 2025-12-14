using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VisionFlow.Application.Interfaces;
using VisionFlow.Infrastructure.Configuration;
using VisionFlow.Infrastructure.Messaging;

namespace VisionFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure RabbitMQ settings
        services.Configure<RabbitMqSettings>(
            configuration.GetSection("RabbitMQ"));

        // Register event publisher
        services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        return services;
    }
}
