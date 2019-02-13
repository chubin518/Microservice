namespace DotNetCore.Microservice.Routing
{
    public interface IRouteCollection : IRouter
    {
        void Add(string name, IRouter router);
    }
}
