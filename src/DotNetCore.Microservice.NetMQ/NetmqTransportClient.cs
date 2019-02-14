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
        private readonly NetMQSocket _clientSocket;
        private readonly ISerializer<string> _serializer;
        private readonly EndPoint _endPoint;
        public NetmqTransportClient(ISerializer<string> serializer, EndPoint endPoint)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
            _clientSocket = new RequestSocket($"tcp://{endPoint}");
        }

        public void Dispose()
        {
            _clientSocket.Dispose();
            NetMQConfig.Cleanup();
        }

        public Task<OwinResponse> SendAsync(OwinRequest request)
        {
            return Task.Factory.StartNew(() =>
            {
                _clientSocket.SendFrame(_serializer.Serialize(request));
                string resultData = _clientSocket.ReceiveFrameString();
                return _serializer.Deserialize<OwinResponse>(resultData);
            });
        }
    }
}
