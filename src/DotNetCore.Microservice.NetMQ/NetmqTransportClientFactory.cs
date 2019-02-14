using System;
using System.Collections.Concurrent;
using System.Net;
namespace DotNetCore.Microservice.NetMQ
{
    public class NetmqTransportClientFactory : ITransportClientFactory
    {
        private ISerializer<string> _serializer;
        private readonly ConcurrentDictionary<EndPoint, ITransportClient> clients = new ConcurrentDictionary<EndPoint, ITransportClient>();

        public NetmqTransportClientFactory(ISerializer<string> serializer)
        {
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public ITransportClient CreateClient(EndPoint endPoint)
        {
            if (clients.TryGetValue(endPoint, out ITransportClient transportClient))
            {
                return transportClient;
            }
            else
            {
                transportClient = new NetmqTransportClient(_serializer, endPoint);
                clients.AddOrUpdate(endPoint, transportClient, (point, client) => transportClient);
                return transportClient;
            }
        }
    }
}
