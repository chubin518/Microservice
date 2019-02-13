using DotNetCore.Microservice.HttpKestrel.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DotNetCore.Microservice.HttpKestrel
{
    public static class HttpKestrelHostExtensions
    {
        public static IHostBuilder UseKestrelServer(this IHostBuilder builder, int port = 10000)
        {
            return builder.ConfigureServices((bulderContext, services) =>
            {
                services.AddMicroCore();
                services.AddMicroServer(port);
                services.TryAddSingleton<IOwinContextFactory, DefaultHttpContextFactory>();
                services.AddSingleton<IHostingServer, HttpKestrelHostingServer>();
            });
        }
    }
}
