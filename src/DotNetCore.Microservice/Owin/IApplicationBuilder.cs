using System;

namespace DotNetCore.Microservice.Owin
{
    public interface IApplicationBuilder
    {
        /// <summary>
        /// 提供对应用程序级别容器的服务的访问。
        /// </summary>
        IServiceProvider ApplicationServices { get; set; }

        /// <summary>
        /// 构建处理请求的委托
        /// </summary>
        /// <returns></returns>
        RequestDelegate Build();

        /// <summary>
        /// 添加一个中间件至请求处理管道
        /// 
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        IApplicationBuilder Use(Func<RequestDelegate,RequestDelegate> middleware);
    }
}
