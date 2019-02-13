using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCore.Microservice
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);

        void Configure(IApplicationBuilder app);
    }
}
