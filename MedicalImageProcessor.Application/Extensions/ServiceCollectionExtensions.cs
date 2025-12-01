using MedicalImageProcessor.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalImageProcessor.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ImageDetectionService>();  // ← Має бути це
            return services;
        }
    }
}