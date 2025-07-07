using Microsoft.Extensions.DependencyInjection;

namespace DaisyMudDomain;

public static class DependencyInjection
{
    public static IServiceCollection AddMudDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<SearchService>();
        return services;
    }
}