using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Owin
{
    /// <summary>
    /// 构建请求处理管道
    /// </summary>
    public class ApplicationBuilder : IApplicationBuilder
    {
        private readonly IList<Func<RequestDelegate, RequestDelegate>> _middlewares = new List<Func<RequestDelegate, RequestDelegate>>();

        public ApplicationBuilder(IServiceProvider services)
        {
            ApplicationServices = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceProvider ApplicationServices { get; set; }

        public RequestDelegate Build()
        {
            RequestDelegate next = context =>
            {
                context.Response = OwinResponse.Nofound();
                return Task.CompletedTask;
            };

            foreach (var middleware in _middlewares.Reverse())
            {
                next = middleware(next);
            }

            return next;
        }

        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            this._middlewares.Add(middleware);
            return this;
        }
    }
}
