using System.Net;

namespace DotNetCore.Microservice
{
    public class HostingOptions
    {
        public HostingOptions()
        {
            this.Host = new IPEndPoint(IPAddress.Any, this.Port);
        }

        public int Port { get; set; } = 10000;

        public IPEndPoint Host { get; }

        //public Action<IApplicationBuilder> ConfigureApp { get; set; }
    }
}
