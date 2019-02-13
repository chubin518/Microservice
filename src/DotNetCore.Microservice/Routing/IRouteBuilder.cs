namespace DotNetCore.Microservice.Routing
{
    public interface IRouteBuilder
    {
        IRouter Build();
    }
}