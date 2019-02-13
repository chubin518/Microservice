using DotNetCore.Microservice;
using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace RpcServer
{
    public class Startup : IStartup
    {
        private IHostingEnvironment _environment;
        public Startup(IHostingEnvironment environment)
        {
            this._environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("Startup ConfigureServices");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.Use((next) => async (context) =>
            {
                context.Items.Add("req", DateTime.Now.ToString());
                await next(context);
                context.Items.Add("res", DateTime.Now.ToString());
            });
        }
    }
}
