using DotNetCore.Microservice.Owin;
using DotNetCore.Microservice.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Internal
{
    public class ServerHostedService : IHostedService
    {
        private readonly IStartup _startup;
        private IServiceProvider _applicationServices;
        private readonly IHostingServer _hostingServer;
        private readonly HostingOptions _hostingOptions;
        private readonly IApplicationBuilder _applicationBuilder;
        private readonly ILogger<ServerHostedService> _logger;
        private readonly IRouteBuilder _routeBuilder;
        public ServerHostedService(
            IStartup startup,
            IHostingServer hostingServer,
            IOptions<HostingOptions> options,
            IServiceProvider applicationServices,
            IApplicationBuilder applicationBuilder,
            ILogger<ServerHostedService> logger,
            IRouteBuilder routeBuilder)
        {
            _startup = startup ?? throw new ArgumentNullException(nameof(_startup));
            _hostingServer = hostingServer ?? throw new ArgumentNullException(nameof(hostingServer));
            _hostingOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            _applicationServices = applicationServices ?? throw new ArgumentNullException(nameof(applicationServices));
            _applicationBuilder = applicationBuilder ?? throw new ArgumentNullException(nameof(applicationBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _routeBuilder = routeBuilder ?? throw new ArgumentNullException(nameof(routeBuilder));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("开始启动消息监听服务");
            try
            {
                HostingApplication hostingApplication = BuildApplication();
                await _hostingServer.StartAsync(hostingApplication, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "应用程序启动异常");
                throw;
            }
            finally
            {
                _logger.LogInformation($"消息监听服务已启动：{_hostingOptions.Host}");
            }
        }

        private HostingApplication BuildApplication()
        {
            _startup.Configure(_applicationBuilder);
            //var configure = _hostingOptions.ConfigureApp ?? throw new InvalidOperationException($"No application configured. Please specify an application via HostBuilder.Configure in the host configuration.");
            //configure(_applicationBuilder);
            _applicationBuilder.Use(next => new RouterMiddleware(next, _routeBuilder.Build()).Invoke);

            RequestDelegate application = _applicationBuilder.Build();

            var contextFactory = _applicationServices.GetRequiredService<IOwinContextFactory>();
            var diagnosticSource = _applicationServices.GetRequiredService<DiagnosticListener>();
            return new HostingApplication(application, contextFactory, diagnosticSource);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                return _hostingServer.StopAsync(cancellationToken);
            }
            finally
            {
                _logger.LogInformation("消息监听服务已终止");
            }
        }
    }
}
