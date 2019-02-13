using Microsoft.Extensions.DependencyInjection;

namespace DotNetCore.Microservice.HttpKestrel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKestrelClient(this IServiceCollection services)
        {
            HttpClientFactoryServiceCollectionExtensions.AddHttpClient(services);
            services.AddMicroCore();
            services.AddMicroClient();
            services.AddSingleton<ITransportClient, HttpTransportClient>();
            return services;
        }
    }
}
