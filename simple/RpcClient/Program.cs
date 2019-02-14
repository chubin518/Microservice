﻿using DotNetCore.Microservice.NetMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RpcContracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RpcClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var builder = new ConfigurationBuilder();
            IServiceCollection serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddNetmqClient()
                    .AddSingleton<IConfiguration>(i => builder.AddJsonFile("app.json").Build());
            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            IHelloService helloService = serviceProvider.GetRequiredService<IHelloService>();
            // 异步请求
            Task.Run(async () =>
            {
                string data = await helloService.HelloAsync("tom");
                Console.WriteLine(data);
            });
            Thread.Sleep(1000);
            // 同步请求
            string result = helloService.Hello("tom");
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
