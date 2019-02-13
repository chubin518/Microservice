using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;

namespace DotNetCore.Microservice
{
    public class TransportClientOptions
    {
        public IDictionary<string, List<ClientHostOption>> Clients { get; set; }
    }

    public class ClientHostOption
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public EndPoint ToEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(Host), Port);
        }
    }
}
