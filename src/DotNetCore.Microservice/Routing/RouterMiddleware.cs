using DotNetCore.Microservice.Owin;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Routing
{
    public class RouterMiddleware : OwinMiddleware
    {
        private IRouter _router;

        public RouterMiddleware(RequestDelegate next, IRouter router) : base(next)
        {
            this._router = router;
        }

        public override async Task Invoke(OwinContext context)
        {
            RouteContext routeContext = new RouteContext(context);
            await _router.RouteAsync(routeContext);
            if (routeContext.Handler != null)
            {
                try
                {
                    await routeContext.Handler(context);
                }
                catch (System.Exception ex)
                {
                    context.Response.Error(ex.ToString());
                    throw;
                }
            }
            else
            {
                await Next(context);
            }
        }
    }
}
