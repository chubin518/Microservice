using DotNetCore.Microservice.Owin;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Net;
using System.Threading.Tasks;
namespace DotNetCore.Microservice.NetMQ
{
    public class NetmqTransportClient : ITransportClient
    {
        private readonly string INPROC_CLIENT_URL ;
        private readonly NetMQSocket _dealerSocket; //连接到服务端
        private readonly NetMQSocket _routerSocket;
        private readonly Proxy _dealerProxy;
        private readonly ISerializer<string> _serializer;
        private readonly EndPoint _endPoint;
        public NetmqTransportClient(ISerializer<string> serializer, EndPoint endPoint)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            INPROC_CLIENT_URL = "inproc://request_dealer" + Guid.NewGuid().ToString();
            _dealerSocket = new DealerSocket($"tcp://{endPoint}");
            _routerSocket = new RouterSocket(INPROC_CLIENT_URL);
            _dealerProxy = new Proxy(_dealerSocket, _routerSocket);
            Task.Factory.StartNew(_dealerProxy.Start);
        }
        public void Dispose()
        {
            _dealerSocket.Dispose();
            _routerSocket.Dispose();
            _dealerProxy.Stop();
        }

        public Task<OwinResponse> SendAsync(OwinRequest request)
        {
            return Task.Factory.StartNew(() =>
            {
                using (NetMQSocket requestSocket = new RequestSocket(INPROC_CLIENT_URL))
                {
                    requestSocket.SendFrame(_serializer.Serialize(request));
                    string resultData = requestSocket.ReceiveFrameString();
                    return _serializer.Deserialize<OwinResponse>(resultData);
                }
            });
        }
    }

    /// <summary>
    /// Request-Response模式： NetMQ.FiniteStateMachineException:“Req.XSend - cannot send another request”
    /// </summary>
    //public class NetmqTransportClient : ITransportClient
    //{
    //    private readonly NetMQSocket _clientSocket;
    //    private readonly ISerializer<string> _serializer;
    //    private readonly EndPoint _endPoint;
    //    public NetmqTransportClient(ISerializer<string> serializer, EndPoint endPoint)
    //    {
    //        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    //        _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
    //        _clientSocket = new RequestSocket($"tcp://{endPoint}");
    //        _clientSocket.Options.Linger = TimeSpan.Zero;
    //    }

    //    public void Dispose()
    //    {
    //        _clientSocket.Dispose();
    //        NetMQConfig.Cleanup(false);
    //    }

    //    public Task<OwinResponse> SendAsync(OwinRequest request)
    //    {
    //        return Task.Factory.StartNew(() =>
    //        {
    //            _clientSocket.SendFrame(_serializer.Serialize(request));
    //            string resultData = _clientSocket.ReceiveFrameString();
    //            return _serializer.Deserialize<OwinResponse>(resultData);
    //        });
    //    }
    //}
}
