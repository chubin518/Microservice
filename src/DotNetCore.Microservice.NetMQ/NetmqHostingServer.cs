using DotNetCore.Microservice.Internal;
using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace DotNetCore.Microservice.NetMQ
{
    public class NetmqHostingServer : IHostingServer
    {
        //进程内通信协议
        private const string INPROC_SERVER_URL = "inproc://response_dealer";
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
        private readonly HostingOptions _hostingOptions;
        private readonly ISerializer<string> _serializer;
        private readonly ILogger<NetmqHostingServer> _logger;
        private NetMQSocket _routerSocket; //负责接收客户端消息
        private NetMQSocket _dealerSocket; //负责与worker通信
        private NetMQPoller _poller;
        private NetMQPoller _workerPoller;

        public NetmqHostingServer(IOptions<HostingOptions> options,
            ISerializer<string> serializer,
            ILogger<NetmqHostingServer> logger)
        {
            _hostingOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void DealerSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage message = e.Socket.ReceiveMultipartMessage();
            _routerSocket.SendMultipartMessage(message);
        }

        private void RouterSocket_ReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            NetMQMessage message = e.Socket.ReceiveMultipartMessage();
            _dealerSocket.SendMultipartMessage(message);
        }

        public Task StartAsync(HostingApplication hostingApplication, CancellationToken cancellationToken)
        {
            _routerSocket = new RouterSocket();
            _routerSocket.Bind($"tcp://{_hostingOptions.Host}");
            _routerSocket.ReceiveReady += RouterSocket_ReceiveReady;
            _dealerSocket = new DealerSocket();
            _dealerSocket.Bind(INPROC_SERVER_URL);
            _dealerSocket.ReceiveReady += DealerSocket_ReceiveReady;

            _poller = new NetMQPoller { _routerSocket, _dealerSocket };
            _poller.RunAsync();
            _workerPoller = new NetMQPoller();
            async void OnReceiveReady(object sender, NetMQSocketEventArgs args)
            {
                NetMQMessage receiveMessage = args.Socket.ReceiveMultipartMessage();
                string address = receiveMessage.Pop().ConvertToString();
                string content = receiveMessage.Last().ConvertToString(Encoding.UTF8);
                OwinContext owinContext = null;
                long startTimestamp = Stopwatch.GetTimestamp();
                _logger.LogInformation($"Request starting tcp://{_hostingOptions.Host} {content}");
                owinContext = hostingApplication.CreateContext(content);
                try
                {
                    await hostingApplication.ProcessRequestAsync(owinContext);
                }
                catch (Exception ex)
                {
                    owinContext.Response.Error(ex.Message);
                    throw;
                }
                finally
                {
                    string sendContent = _serializer.Serialize(owinContext.Response);
                    _logger.LogInformation($"Request finished in {new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - startTimestamp))).TotalMilliseconds}ms {sendContent}");
                    NetMQMessage sendMessage = new NetMQMessage();
                    sendMessage.Append(address);
                    sendMessage.AppendEmptyFrame();
                    sendMessage.Append(sendContent, Encoding.UTF8);
                    args.Socket.SendMultipartMessage(sendMessage);
                    hostingApplication.DisponseContext(owinContext);
                }
            }

            foreach (var item in Enumerable.Range(0, _hostingOptions.ProcessCount))
            {
                NetMQSocket process = new DealerSocket();
                process.Connect(INPROC_SERVER_URL);
                process.ReceiveReady += OnReceiveReady;
                _workerPoller.Add(process);
            }

            _workerPoller.RunAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _routerSocket.Dispose();
            _dealerSocket.Dispose();
            _poller.StopAsync();
            _workerPoller.StopAsync();
            NetMQConfig.Cleanup(false);
            return Task.CompletedTask;
        }
    }

    // 并发请求时性能就不佳了
    //public class NetmqHostingServer : IHostingServer
    //{
    //    private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
    //    private readonly NetMQSocket _serverSocket;
    //    private readonly NetMQPoller _poller;
    //    private readonly HostingOptions _hostingOptions;
    //    private readonly ISerializer<string> _serializer;
    //    private readonly ILogger<NetmqHostingServer> _logger;

    //    public NetmqHostingServer(IOptions<HostingOptions> options,
    //        ISerializer<string> serializer,
    //        ILogger<NetmqHostingServer> logger)
    //    {
    //        _hostingOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
    //        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    //        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    //        _serverSocket = new ResponseSocket($"tcp://{_hostingOptions.Host}");
    //        _serverSocket.Options.TcpKeepalive = true;
    //        _serverSocket.Options.TcpKeepaliveInterval = new TimeSpan(0, 0, 55);
    //        _serverSocket.Options.TcpKeepaliveIdle = new TimeSpan(0, 0, 25);
    //        _poller = new NetMQPoller { _serverSocket };
    //    }

    //    Task IHostingServer.StartAsync(HostingApplication hostingApplication, CancellationToken cancellationToken)
    //    {
    //        _serverSocket.ReceiveReady += async (sender, args) =>
    //        {
    //            string message = _serverSocket.ReceiveFrameString(Encoding.UTF8);
    //            OwinContext owinContext = null;
    //            long startTimestamp = Stopwatch.GetTimestamp();
    //            try
    //            {
    //                _logger.LogInformation($"Request starting tcp://{_hostingOptions.Host} {message}");
    //                owinContext = hostingApplication.CreateContext(message);
    //                await hostingApplication.ProcessRequestAsync(owinContext);
    //            }
    //            catch (Exception ex)
    //            {
    //                owinContext.Response = OwinResponse.Error(ex.Message);
    //                throw;
    //            }
    //            finally
    //            {
    //                string text = _serializer.Serialize(owinContext.Response);
    //                _logger.LogInformation($"Request finished in {new TimeSpan((long)(TimestampToTicks * (Stopwatch.GetTimestamp() - startTimestamp))).TotalMilliseconds}ms {text}");
    //                _serverSocket.SendFrame(text);
    //                hostingApplication.DisponseContext(owinContext);
    //            }
    //        };
    //        _poller.RunAsync();
    //        return Task.CompletedTask;
    //    }

    //    Task IHostingServer.StopAsync(CancellationToken cancellationToken)
    //    {
    //        _poller.Dispose();
    //        _serverSocket.Dispose();
    //        NetMQConfig.Cleanup(false);
    //        return Task.CompletedTask;
    //    }
    //}
}
