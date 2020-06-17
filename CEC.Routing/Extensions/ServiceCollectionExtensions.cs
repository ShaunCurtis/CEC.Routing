using CEC.Routing.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CEC.Routing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCECRouting(this IServiceCollection services)
        {
            services.AddScoped<RouterSessionService>();
            return services;
        }
    }
}
