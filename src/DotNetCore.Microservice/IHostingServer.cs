using DotNetCore.Microservice.Internal;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetCore.Microservice
{
    /// <summary>
    /// 托管应用的服务
    /// </summary>
    public interface IHostingServer
    {
        /// <summary>
        /// 启动应用程序
        /// </summary>
        /// <param name="hostingApplication"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartAsync(HostingApplication hostingApplication, CancellationToken cancellationToken);

        /// <summary>
        /// 停止应用程序
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}
