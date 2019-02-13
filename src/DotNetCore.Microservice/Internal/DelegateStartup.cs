using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotNetCore.Microservice.Internal
{
    public class DelegateStartup : IStartup
    {
        private Action<IApplicationBuilder> _configure;

        public DelegateStartup(Action<IApplicationBuilder> configure)
        {
            this._configure = configure;
        }

        public void Configure(IApplicationBuilder app)
        {
            _configure(app);
        }

        public void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
