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
                    string data = await helloService.HelloAsync("1");
                    Console.WriteLine(data);
                }));
                //Task.Factory.StartNew(() =>
                //{
                //    string result = helloService.Hello("2");
                //    Console.WriteLine(result);
                //});
            });

            Console.ReadKey();
        }
    }
}
