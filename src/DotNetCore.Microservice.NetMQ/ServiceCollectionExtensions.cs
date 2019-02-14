using Microsoft.Extensions.DependencyInjection;

namespace DotNetCore.Microservice.NetMQ
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetmqClient(this IServiceCollection services)
        {
            services.AddMicroCore();
            services.AddMicroClient();
            services.AddSingleton<ITransportClientFactory, NetmqTransportClientFactory>();
            return services;
        }
    }
}
