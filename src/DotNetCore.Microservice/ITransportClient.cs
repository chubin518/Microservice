using DotNetCore.Microservice.Owin;
using System;
using System.Net;
using System.Threading.Tasks;

namespace DotNetCore.Microservice
{
    public interface ITransportClient : IDisposable
    {
        Task<OwinResponse> SendAsync(OwinRequest request);
    }
}
