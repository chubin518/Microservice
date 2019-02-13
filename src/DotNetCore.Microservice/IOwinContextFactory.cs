

using DotNetCore.Microservice.Owin;

namespace DotNetCore.Microservice
{
    public interface IOwinContextFactory
    {
        /// <summary>
        /// 创建默认的请求上线文对象
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        OwinContext Create(object feature);

        /// <summary>
        /// 清理、释放上下文内容
        /// </summary>
        /// <param name="context"></param>
        void Dispose(OwinContext context);
    }
}
