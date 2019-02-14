using DotNetCore.Microservice.Internal;
using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.NetMQ
{
    public class NetmqHostingServer : IHostingServer
    {
        private readonly NetMQSocket _serverSocket;
        private readonly NetMQPoller _poller;
        private readonly HostingOptions _hostingOptions;
        private readonly ISerializer<string> _serializer;
        private readonly ILogger<NetmqHostingServer> _logger;

        public NetmqHostingServer(IOptions<HostingOptions> options,
            ISerializer<string> serializer,
            ILogger<NetmqHostingServer> logger)
        {
            _hostingOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _serverSocket = new ResponseSocket($"tcp://{_hostingOptions.Host}");
            _serverSocket.Options.TcpKeepalive = true;
            _serverSocket.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 55);
            _serverSocket.Options.TcpKeepaliveIdle = new TimeSpan(0, 0, 25);
            _poller = new NetMQPoller { _serverSocket };
        }

        Task IHostingServer.StartAsync(HostingApplication hostingApplication, CancellationToken cancellationToken)
        {
            _serverSocket.ReceiveReady += async (sender, args) =>
            {
                string message = _serverSocket.ReceiveFrameString(Encoding.UTF8);
                OwinContext owinContext = null;
                try
                {
                    _logger.LogInformation(message);
                    owinContext = hostingApplication.CreateContext(message);
                    await hostingApplication.ProcessRequestAsync(owinContext);
                }
                catch (Exception ex)
                {
                    owinContext.Response = OwinResponse.Error(ex.Message);
                    throw;
                }
                finally
                {
                    string text = _serializer.Serialize(owinContext.Response);
                    _serverSocket.SendFrame(text);
                    hostingApplication.DisponseContext(owinContext);
                }
            };
            _poller.RunAsync();
            return Task.CompletedTask;
        }

        Task IHostingServer.StopAsync(CancellationToken cancellationToken)
        {
            _poller.Dispose();
            _serverSocket.Dispose();
            NetMQConfig.Cleanup();
            return Task.CompletedTask;
        }
    }
}
