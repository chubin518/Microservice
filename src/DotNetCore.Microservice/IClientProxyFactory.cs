using System;

namespace DotNetCore.Microservice
{
    public interface IClientProxyFactory
    {
        /// <summary>
        /// 创建代理类实例
        /// </summary>
        /// <param name="targetType">要代理的接口</param>
        /// <returns></returns>
        object CreateProxy(Type targetType);

        /// <summary>
        /// 创建代理类实例
        /// </summary>
        /// <typeparam name="TTarget">要代理的接口</typeparam>
        /// <param name="parameters">拦截器构造函数参数值</param>
        /// <returns></returns>
        TTarget CreateProxy<TTarget>();
    }
}
