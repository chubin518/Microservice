using System.Threading.Tasks;

namespace DotNetCore.Microservice.Routing
{
    public interface IRouter
    {
        Task RouteAsync(RouteContext context);
    }
}
