using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Routing
{
    public class RouteCollection : IRouteCollection
    {
        private readonly IDictionary<string, IRouter> _routers = new Dictionary<string, IRouter>(StringComparer.OrdinalIgnoreCase);

        public void Add(string name, IRouter router)
        {
            if (_routers.ContainsKey(name ?? throw new ArgumentNullException(nameof(name))))
            {
                throw new InvalidOperationException($"已存在名称为：{name} 的服务路由");
            }
            _routers.Add(name, router ?? throw new ArgumentNullException(nameof(router)));
        }

        public Task RouteAsync(RouteContext context)
        {
            if (!string.IsNullOrWhiteSpace(context.Path) && _routers.TryGetValue(context.Path, out IRouter router))
            {
                return router.RouteAsync(context);
            }
            return Task.CompletedTask;
        }
    }
}
