using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System;

namespace DotNetCore.Microservice.HttpKestrel
{
    public class HttpTransportClientFactory : ITransportClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISerializer<string> _serializer;

        private readonly ConcurrentDictionary<EndPoint, ITransportClient> clients = new ConcurrentDictionary<EndPoint, ITransportClient>();

        public HttpTransportClientFactory(IHttpClientFactory httpClientFactory, ISerializer<string> serializer)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
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
                transportClient = new HttpTransportClient(_httpClientFactory, _serializer, endPoint);
                clients.AddOrUpdate(endPoint, transportClient, (point, client) => transportClient);
                return transportClient;
            }
        }
    }
}
