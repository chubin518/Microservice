using System.Net;

namespace DotNetCore.Microservice
{
    public interface ITransportClientFactory
    {
        ITransportClient CreateClient(EndPoint endPoint);
    }
}
