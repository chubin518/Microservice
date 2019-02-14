using DotNetCore.Microservice.Owin;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DotNetCore.Microservice.NetMQ
{
    public class NetmqContextFactory : IOwinContextFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISerializer<string> _serializer;
        public NetmqContextFactory(IServiceProvider serviceProvider, ISerializer<string> serializer)
        {
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public OwinContext Create(object feature)
        {
            string formData = (feature as string) ?? throw new InvalidOperationException("当前上线文信息为null或类型错误");
            return new OwinContext()
            {
                RequestServices = _serviceScopeFactory.CreateScope().ServiceProvider,
                Request = _serializer.Deserialize<OwinRequest>(formData),
                Response = new OwinResponse()
            };
        }

        public void Dispose(OwinContext context)
        {
            context = null;
        }
    }
}
