using DotNetCore.Microservice.Owin;

namespace DotNetCore.Microservice.Routing
{
    public class RouteContext
    {
        public RouteContext(OwinContext context)
        {
            Context = context;
            Path = context.Request.Path;
            Group = context.Request.Group;
        }

        public string Group { get; }

        public string Path { get; }

        public OwinContext Context { get; }

        public RequestDelegate Handler { get; set; }
    }
}
