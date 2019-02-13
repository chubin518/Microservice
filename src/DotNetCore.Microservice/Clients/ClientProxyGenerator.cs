using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Clients
{
    public class ClientProxyGenerator : DispatchProxyAsync
    {
        private ITransportDispatcher _dispatcher;

        /// <summary>
        /// 创建代理类实例
        /// </summary>
        /// <param name="targetType">要代理的接口</param>
        /// <param name="dispatcher">拦截器</param>
        /// <returns></returns>
        public static object CreateInstance(Type targetType, ITransportDispatcher dispatcher)
        {
            object proxy = CreateProxy(targetType);
            ((ClientProxyGenerator)proxy)._dispatcher = dispatcher;
            return proxy;
        }

        public override object Invoke(MethodInfo method, object[] args)
        {
            return _dispatcher.Dispatch<object>(method, args);
        }

        public override Task InvokeAsync(MethodInfo method, object[] args)
        {
            throw new NotImplementedException();
        }

        public override Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
        {
            return _dispatcher.DispatchAsync<T>(method, args);
        }

        /// <summary>
        /// 创建代理类
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        private static object CreateProxy(Type targetType)
        {
            MethodCallExpression callexp = Expression.Call(typeof(DispatchProxyAsync), nameof(DispatchProxyAsync.Create), new[] { targetType, typeof(ClientProxyGenerator) });
            return Expression.Lambda<Func<object>>(callexp).Compile()();
        }
    }
}
