using DotNetCore.Microservice;
using System.Threading.Tasks;

namespace RpcContracts
{
    [Service]
    public interface IHelloService
    {
        string Hello(string name);

        Task<string> HelloAsync(string name);
    }
}
