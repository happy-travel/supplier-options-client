using HappyTravel.SupplierOptionsClient.Infrastructure;
using HappyTravel.SupplierOptionsClient.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.SupplierOptionsClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSupplierOptionsClient(this IServiceCollection services, 
        Action<SupplierOptionsClientSettings> configurationAction, string identityClientName)
    {
        services.AddTransient<ISupplierOptionsClient, SupplierOptionsClient>();
        services.Configure(configurationAction);
        services.AddClientAccessTokenHttpClient(HttpClientNames.SupplierOptionsClient, identityClientName, (Action<HttpClient>) null!);
        
        return services;
    }
}