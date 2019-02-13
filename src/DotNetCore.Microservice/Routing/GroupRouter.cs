using DotNetCore.Microservice.Owin;
using System;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Routing
{
    public class GroupRouter : IRouter
    {
        private string _group;
        private IRouter _router;

        public GroupRouter(IRouter router, string group)
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _group = group ?? throw new ArgumentNullException(nameof(group));
        }

        public Task RouteAsync(RouteContext context)
        {
            if (!string.Equals(context.Group, this._group, StringComparison.OrdinalIgnoreCase))
            {
                context.Handler = (ctx) =>
                {
                    ctx.Response = OwinResponse.Nofound($"can not match service group {context.Group}");
                    return Task.CompletedTask;
                };
                return Task.CompletedTask;
            }
            return _router.RouteAsync(context);
        }
    }
}
