using DotNetCore.Microservice.Metadata;
using System;
namespace DotNetCore.Microservice.Routing
{
    public class RouteBuilder : IRouteBuilder
    {
        private readonly ServiceMetadata _metadata;

        public RouteBuilder(ServiceMetadata metadata)
        {
            this._metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public IRouter Build()
        {
            var routeCollection = new RouteCollection();
            IRouter router;
            foreach (var service in _metadata.Services)
            {
                foreach (var method in service.Methods)
                {
                    router = new ServiceRouter(service.Service, service.Implementation, method);
                    router = new GroupRouter(router, service.Group);
                    routeCollection.Add(RoutePath.Parse(method), router);
                }
            }
            return routeCollection;
        }
    }
}
