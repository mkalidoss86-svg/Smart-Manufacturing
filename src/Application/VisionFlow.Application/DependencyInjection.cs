using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using VisionFlow.Application.Services;
using VisionFlow.Application.Validators;

namespace VisionFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register validators
        services.AddValidatorsFromAssemblyContaining<ProductionQualityEventValidator>();

        // Register services
        services.AddScoped<QualityEventService>();

        return services;
    }
}
