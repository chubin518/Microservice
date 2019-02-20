using Microsoft.Extensions.Logging;
using RpcContracts;
using System;
using System.Threading.Tasks;

namespace RpcServer
{
    public class HelloService : IHelloService
    {
        public ILogger<HelloService> _logger;

        public HelloService(ILoggerFactory logger)
        {
            _logger = logger.CreateLogger<HelloService>();
        }
        public string Hello(string name)
        {
            return $"hello {name}";
        }

        public async Task<string> HelloAsync(string name)
        {
            _logger.LogInformation(name + "————" + DateTime.Now.ToLongTimeString());
            string result = Hello(name);
            await Task.Delay(5000);
            return result + " 我是异步请求";
        }
    }
}
