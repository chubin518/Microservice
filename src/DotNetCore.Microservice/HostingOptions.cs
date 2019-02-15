using System.Net;

namespace DotNetCore.Microservice
{
    public class HostingOptions
    {
        public HostingOptions()
        {
            this.Host = new IPEndPoint(IPAddress.Any, this.Port);
        }

        /// <summary>
        /// 服务监听端口
        /// </summary>
        public int Port { get; set; } = 10000;

        /// <summary>
        /// 响应处理线程数
        /// </summary>
        public int ProcessCount { get; set; } = 3;

        public IPEndPoint Host { get; }

        //public Action<IApplicationBuilder> ConfigureApp { get; set; }
    }
}
