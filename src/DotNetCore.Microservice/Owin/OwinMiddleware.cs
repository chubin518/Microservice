using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Owin
{
    /// <summary>
    /// 中间件
    /// </summary>
    public abstract class OwinMiddleware
    {
        protected OwinMiddleware(RequestDelegate next)
        {
            Next = next;
        }
        /// <summary>
        /// 下一个组件
        /// </summary>
        protected RequestDelegate Next { get; set; }

        /// <summary>
        /// 处理下一个请求或终止请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public abstract Task Invoke(OwinContext context);
    }
}
