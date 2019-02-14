using DotNetCore.Microservice.Owin;
using DotNetCore.Microservice.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Clients
{
    public class DefaultTransportDispatcher : ITransportDispatcher
    {
        private ITransportClientFactory _transportClientFactory;
        private TransportClientOptions _clientOptions;
        public DefaultTransportDispatcher(ITransportClientFactory transportClientFactory,
            TransportClientOptions clientOptions)
        {
            _transportClientFactory = transportClientFactory ?? throw new ArgumentNullException(nameof(transportClientFactory));
            _clientOptions = clientOptions ?? throw new ArgumentNullException(nameof(clientOptions));
        }

        public TResult Dispatch<TResult>(MethodInfo method, object[] args)
        {
            return this.DispatchAsync<TResult>(method, args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<TResult> DispatchAsync<TResult>(MethodInfo method, object[] args)
        {
            ServiceAttribute attribute = method.DeclaringType.GetCustomAttribute<ServiceAttribute>();
            OwinRequest request = new OwinRequest()
            {
                Path = RoutePath.Parse(method),
                Parameters = args,
                Group = string.IsNullOrWhiteSpace(attribute.Group) ? method.DeclaringType.Assembly.GetName().Name : attribute.Group
            };
            return await this.SendAsync<TResult>(request);
        }

        private async Task<TResult> SendAsync<TResult>(OwinRequest request)
        {
            if (!_clientOptions.Clients.TryGetValue(request.Group, out List<ClientHostOption> lstClient))
            {
                throw new KeyNotFoundException($"No find Rpc client config for {request.Group}");
            }
            if (lstClient != null && lstClient.Any())
            {
                int index = new Random().Next(0, lstClient.Count);
                var response = await _transportClientFactory.CreateClient(lstClient[index].ToEndPoint()).SendAsync(request);
                return (TResult)response.ReturnValue;
            }
            else
            {
                throw new NotSupportedException($"No find server endpoints for supported {request.Group}");
            }
        }

    }
}
