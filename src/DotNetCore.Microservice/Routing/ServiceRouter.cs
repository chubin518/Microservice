using DotNetCore.Microservice.Core;
using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Routing
{
    public class ServiceRouter : IRouter
    {
        private readonly ObjectMethodExecutor _methodExecutor;

        public ServiceRouter(TypeInfo service, TypeInfo implementation, MethodInfo method)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            _methodExecutor = ObjectMethodExecutor.Create(method ?? throw new ArgumentNullException(nameof(method)), implementation ?? throw new ArgumentNullException(nameof(implementation)));
        }

        public TypeInfo Service { get; private set; }

        public Task RouteAsync(RouteContext route)
        {
            if (route.Handler == null)
            {
                route.Handler = async (context) =>
                {
                    object instance = context.RequestServices.GetRequiredService(this.Service);
                    object[] methodArgs = context.Request.Parameters;
                    object resultObj;
                    if (_methodExecutor.IsMethodAsync)
                    {
                        resultObj = await _methodExecutor.ExecuteAsync(instance, methodArgs);
                    }
                    else
                    {
                        resultObj = _methodExecutor.Execute(instance, methodArgs);
                    }
                    context.Response = OwinResponse.Success(resultObj);
                };
            }
            return Task.CompletedTask;
        }
    }
}
