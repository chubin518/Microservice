using System.Threading.Tasks;

namespace DotNetCore.Microservice.Owin
{
    public delegate Task RequestDelegate(OwinContext context);
}
