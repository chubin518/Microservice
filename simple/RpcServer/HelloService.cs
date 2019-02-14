using RpcContracts;
using System.Threading.Tasks;

namespace RpcServer
{
    public class HelloService : IHelloService
    {
        public string Hello(string name)
        {
            return $"hello {name}";
        }

        public async Task<string> HelloAsync(string name)
        {
            string result = Hello(name);
            await Task.Delay(5000);
            return result + " 我是异步请求";
        }
    }
}
