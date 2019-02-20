using DotNetCore.Microservice.Internal;
using NetMQ;
using NetMQ.Sockets;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace DotNetCore.Microservice.NetMQ
{
    public class ZeroMQHostingServer : IHostingServer
    {
        private const string INPROC_SERVER_URL = "inproc://response_dealer";
        private INetMQPoller _poller;
        private NetMQSocket _routerSocket;
        private NetMQSocket _dealerSocket;

        private readonly HostingOptions _hostingOptions;

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
       
            return Task.CompletedTask;
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _poller.StopAsync();
            return Task.CompletedTask;
        }
    }
}
