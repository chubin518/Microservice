using DotNetCore.Microservice.Diagnostics;
using DotNetCore.Microservice.Owin;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Internal
{
    /// <summary>
    /// 处理请求的应用
    /// </summary>
    public class HostingApplication
    {
        private readonly RequestDelegate _application;
        private readonly IOwinContextFactory _contextFactory;
        private readonly DiagnosticListener _diagnosticListener;

        public HostingApplication(
            RequestDelegate application,
            IOwinContextFactory contextFactory,
            DiagnosticListener diagnosticSource)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _diagnosticListener = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
        }

        /// <summary>
        /// 创建当前请求的上下文信息
        /// </summary>
        /// <typeparam name="TFeatures"></typeparam>
        /// <param name="features"></param>
        /// <returns></returns>
        public OwinContext CreateContext<TFeatures>(TFeatures features)
        {
            OwinContext context = _contextFactory.Create(features);
            _diagnosticListener.BeginRequest(context);
            return context;
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ProcessRequestAsync(OwinContext context)
        {
            await _application(context);
        }

        /// <summary>
        /// 清理释放当前请求的上下文信息
        /// </summary>
        /// <param name="context"></param>
        public void DisponseContext(OwinContext context)
        {
            _diagnosticListener.EndRequest(context);
            _contextFactory.Dispose(context);
            context = null;
        }
    }
}
