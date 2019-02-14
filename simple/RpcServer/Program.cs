using DotNetCore.Microservice;
using DotNetCore.Microservice.HttpKestrel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetCore.Microservice.NetMQ;

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
                     //.UseKestrelServer()
                     .UseNetmqServer()
                     .UseStartup<Startup>()
                     .Build()
                     .Run();
        }
    }
}
