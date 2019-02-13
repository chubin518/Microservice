using System.Reflection;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Clients
{
    public interface ITransportDispatcher
    {
        TResult Dispatch<TResult>(MethodInfo targetMethod, object[] args);

        Task<TResult> DispatchAsync<TResult>(MethodInfo targetMethod, object[] args);
    }
}
