using DotNetCore.Microservice.Internal;
using DotNetCore.Microservice.Owin;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.HttpKestrel
{
    public class HttpKestrelHostingServer : IHostingServer
    {
        private IWebHost _webHost;
        private readonly HostingOptions _hostingOptions;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<ILoggerProvider> _loggerProviders;
        private readonly ISerializer<string> _serializer;
        public HttpKestrelHostingServer(IOptions<HostingOptions> options,
            IConfiguration configuration,
            ISerializer<string> serializer,
            IEnumerable<ILoggerProvider> loggerProviders)
        {
            _hostingOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerProviders = loggerProviders ?? throw new ArgumentNullException(nameof(loggerProviders));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
        public async Task StartAsync(HostingApplication hostingApplication, CancellationToken cancellationToken)
        {
            _webHost = new WebHostBuilder()
                .UseKestrel()
                .UseUrls($"http://{_hostingOptions.Host}")
                .UseConfiguration(_configuration)
                .ConfigureLogging(builder =>
                {
                    foreach (var provider in _loggerProviders)
                    {
                        builder.AddProvider(provider);
                    }
                })
                .Configure(app =>
                {
                    app.Use(IgnoreFavicon());
                    app.Use(RequestDispatch(hostingApplication, cancellationToken));
                })
                .Build();
            await _webHost.StartAsync(cancellationToken);
        }

        private Func<Microsoft.AspNetCore.Http.RequestDelegate, Microsoft.AspNetCore.Http.RequestDelegate> RequestDispatch(HostingApplication hostingApplication, CancellationToken cancellationToken)
        {
            return next => async (context) =>
            {
                context.Response.ContentType = "application/json";
                OwinContext owinContext = null;
                try
                {
                    owinContext = hostingApplication.CreateContext(context);
                    await hostingApplication.ProcessRequestAsync(owinContext);
                }
                catch (Exception ex)
                {
                    owinContext.Response = OwinResponse.Error(ex.Message);
                    throw;
                }
                finally
                {
                    context.Response.StatusCode = owinContext.Response.StatusCode;
                    string text = _serializer.Serialize(owinContext.Response);
                    await context.Response.WriteAsync(text, Encoding.UTF8, cancellationToken);
                    hostingApplication.DisponseContext(owinContext);
                }
            };
        }

        private static Func<Microsoft.AspNetCore.Http.RequestDelegate, Microsoft.AspNetCore.Http.RequestDelegate> IgnoreFavicon()
        {
            return next => async (context) =>
            {
                if (context.Request.Path.Value.EndsWith("favicon.ico"))
                {
                    // Pesky browsers
                    context.Response.StatusCode = 404;
                    return;
                }
                await next(context);
            };
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _webHost.StopAsync(cancellationToken);
        }
    }
}
