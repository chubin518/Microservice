using System.Net;

namespace DotNetCore.Microservice.HttpKestrel
{
    public class HttpTransportClientFactory : ITransportClientFactory
    {
        public ITransportClient CreateClient(EndPoint endPoint)
        {
            throw new System.NotImplementedException();
        }
    }
}
