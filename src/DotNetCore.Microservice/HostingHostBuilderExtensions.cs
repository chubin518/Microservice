using DotNetCore.Microservice.Clients;
using DotNetCore.Microservice.Internal;
using DotNetCore.Microservice.Metadata;
using DotNetCore.Microservice.Owin;
using DotNetCore.Microservice.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace DotNetCore.Microservice
{
    public static class HostingHostBuilderExtensions
    {
        public static IHostBuilder Configure(this IHostBuilder hostBuilder, Action<IApplicationBuilder> configure)
        {
            var startupAssemblyName = configure.GetMethodInfo().DeclaringType.GetTypeInfo().Assembly.GetName().Name;
            hostBuilder.UseSetting(HostDefaults.ApplicationKey, startupAssemblyName);

            return hostBuilder.ConfigureServices(collection =>
            {
                //collection.Configure<HostingOptions>(options =>
                //{
                //    options.ConfigureApp = configure;
                //});
                collection.AddSingleton(typeof(IStartup), new DelegateStartup(configure));
            });
        }

        public static IHostBuilder UseSetting(this IHostBuilder hostBuilder, string key, string value)
        {
            return hostBuilder.ConfigureHostConfiguration(builder =>
            {
                builder.AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string>(key ?? throw new ArgumentNullException(nameof(key)),value ?? throw new ArgumentNullException(nameof(value)))
                });
            });
        }

        public static IHostBuilder UseStartup<TStartup>(this IHostBuilder hostBuilder) where TStartup : IStartup
        {
            Type startupType = typeof(TStartup);
            var startupAssemblyName = startupType.GetTypeInfo().Assembly.GetName().Name;
            hostBuilder.UseSetting(HostDefaults.ApplicationKey, startupAssemblyName);
            return hostBuilder.ConfigureServices((context, services) =>
            {
                IStartup startup = ActivatorUtilities.CreateInstance<TStartup>(new HostServiceProvider(context));
                startup.ConfigureServices(services);
                services.AddSingleton(typeof(IStartup), startup);
            });
        }

        // This exists just so that we can use ActivatorUtilities.CreateInstance on the Startup class
        private class HostServiceProvider : IServiceProvider
        {
            private readonly HostBuilderContext _context;
            public HostServiceProvider(HostBuilderContext context)
            {
                _context = context;
            }

            public object GetService(Type serviceType)
            {
                // The implementation of the HostingEnvironment supports both interfaces
                if (serviceType == typeof(Microsoft.Extensions.Hosting.IHostingEnvironment) || serviceType == typeof(IHostingEnvironment))
                {
                    return _context.HostingEnvironment;
                }

                if (serviceType == typeof(IConfiguration))
                {
                    return _context.Configuration;
                }

                return null;
            }
        }
    }
}
