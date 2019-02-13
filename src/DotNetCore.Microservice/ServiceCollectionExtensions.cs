using DotNetCore.Microservice.Clients;
using DotNetCore.Microservice.Diagnostics;
using DotNetCore.Microservice.Internal;
using DotNetCore.Microservice.Metadata;
using DotNetCore.Microservice.Owin;
using DotNetCore.Microservice.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
namespace DotNetCore.Microservice
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 核心服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMicroCore(this IServiceCollection services)
        {
            var listener = new DiagnosticListener(DiagnosticListenerExtensions.DiagnosticListenerName);
            services.AddSingleton<DiagnosticListener>(listener);
            services.AddSingleton<DiagnosticSource>(listener);
            ServiceMetadataBuilder metadataBuilder = new ServiceMetadataBuilder(services);
            services.AddSingleton(metadataBuilder.Build());
            services.AddSingleton<ISerializer<string>, JsonSerializer>();
            return services;
        }

        /// <summary>
        /// 客户端服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMicroClient(this IServiceCollection services)
        {
            services.AddSingleton<ITransportDispatcher, DefaultTransportDispatcher>();
            services.AddSingleton<IClientProxyFactory, DefaultClientProxyFactory>();
            services.TryAddSingleton(provider =>
            {
                TransportClientOptions clientOptions = new TransportClientOptions();
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                configuration.Bind("Services", clientOptions);
                return clientOptions;
            });
            return services;
        }

        /// <summary>
        /// 服务端服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMicroServer(this IServiceCollection services, int port = 10000)
        {
            services.AddSingleton<IApplicationBuilder>(sp => new ApplicationBuilder(sp));
            services.AddHostedService<ServerHostedService>();
            services.AddSingleton<IRouteBuilder, RouteBuilder>();
            services.Configure<HostingOptions>(option =>
            {
                option.Port = port;
            });
            return services;
        }
    }
}
