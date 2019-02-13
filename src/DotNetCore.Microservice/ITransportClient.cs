using DotNetCore.Microservice.Owin;
using System.Net;
using System.Threading.Tasks;

namespace DotNetCore.Microservice
{
    public interface ITransportClient
    {
        Task<OwinResponse> SendAsync(EndPoint end, OwinRequest request);
    }
}
