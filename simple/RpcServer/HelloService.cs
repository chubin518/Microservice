using RpcContracts;
using System;
using System.Threading.Tasks;

namespace RpcServer
{
    public class HelloService : IHelloService
    {
        public string Hello(string name)
        {
            return $"hello {name}";
        }

        public Task<string> HelloAsync(string name)
        {
            string result = Hello(name);
            return Task.FromResult(result+" 我是异步请求");
        }
    }
}
