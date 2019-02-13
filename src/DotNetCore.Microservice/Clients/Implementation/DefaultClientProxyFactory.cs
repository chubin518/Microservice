using System;

namespace DotNetCore.Microservice.Clients
{
    public class DefaultClientProxyFactory : IClientProxyFactory
    {
        private ITransportDispatcher _dispatcher;
        public DefaultClientProxyFactory(ITransportDispatcher dispatcher)
        {
            this._dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public object CreateProxy(Type targetType)
        {
            return ClientProxyGenerator.CreateInstance(targetType, this._dispatcher);
        }

        public TTarget CreateProxy<TTarget>()
        {
            return (TTarget)ClientProxyGenerator.CreateInstance(typeof(TTarget), this._dispatcher);
        }
    }
}
