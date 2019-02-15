using System;
using System.Collections.Concurrent;
using System.Net;

namespace DotNetCore.Microservice.NetMQ
{
    /// <summary>
    /// https://www.cnblogs.com/CreateMyself/p/6086752.html
    /// </summary>
    public class NetmqTransportClientFactory : ITransportClientFactory
    {
        private readonly ISerializer<string> _serializer;
        private static readonly ConcurrentDictionary<EndPoint, ITransportClient> clients = new ConcurrentDictionary<EndPoint, ITransportClient>();
        public NetmqTransportClientFactory(ISerializer<string> serializer)
        {
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public ITransportClient CreateClient(EndPoint endPoint)
        {
            return clients.GetOrAdd(endPoint, (point) =>
            {
                Console.WriteLine(point);
                return new NetmqTransportClient(_serializer, endPoint);
            });
        }
    }
}
