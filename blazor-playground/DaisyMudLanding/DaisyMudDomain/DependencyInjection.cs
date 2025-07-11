using Microsoft.Extensions.DependencyInjection;

namespace DaisyMudDomain;

public static class DependencyInjection
{
    public static IServiceCollection AddMudDomainServices(this IServiceCollection services)
    {
        services.AddScoped<SearchService>();
        services.AddScoped<CountryListService>();
        services.AddScoped<StatsService>();
        services.AddScoped<TestimonialService>();
        services.AddScoped<NotificationService>();
        return services;
    }
}