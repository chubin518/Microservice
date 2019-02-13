using DotNetCore.Microservice;
using DotNetCore.Microservice.HttpKestrel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace RpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new HostBuilder()
                .ConfigureLogging(configure =>
                     {
                         configure.AddConsole();
                         //configure.AddDebug();
                     })
                     .UseKestrelServer()
                     .UseStartup<Startup>()
                     .Build()
                     .Run();
        }
    }
}
