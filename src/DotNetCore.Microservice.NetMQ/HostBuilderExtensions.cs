using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotNetCore.Microservice.NetMQ
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseNetmqServer(this IHostBuilder builder, int port = 10000)
        {
            return builder.ConfigureServices((bulderContext, services) =>
            {
                services.AddMicroCore();
                services.AddMicroServer(port);
                services.TryAddSingleton<IOwinContextFactory, NetmqContextFactory>();
                services.AddSingleton<IHostingServer, NetmqHostingServer>();
            });
        }
    }
}
