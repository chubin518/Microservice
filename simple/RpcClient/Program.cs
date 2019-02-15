using DotNetCore.Microservice.NetMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RpcContracts;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace RpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder();
            IServiceCollection serviceDescriptors = new ServiceCollection();
            serviceDescriptors
                //.AddKestrelClient()
                .AddNetmqClient()
                .AddSingleton<IConfiguration>(i => builder.AddJsonFile("app.json").Build());
            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            IHelloService helloService = serviceProvider.GetRequiredService<IHelloService>();
            Enumerable.Range(0, 1000).ToList().ForEach(item =>
            {
                Task.Factory.StartNew((async () =>
                {
                    string data = await helloService.HelloAsync("tom");
                    Console.WriteLine(data);
                    await Task.Delay(1000);
                    string result = helloService.Hello("tom");
                    Console.WriteLine(result);
                }));
            });

            Console.ReadKey();
        }
    }
}
