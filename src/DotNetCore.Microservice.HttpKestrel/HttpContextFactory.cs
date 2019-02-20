using DotNetCore.Microservice.Owin;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http.Headers;

namespace DotNetCore.Microservice.HttpKestrel.Internal
{
    public class HttpContextFactory : IOwinContextFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISerializer<string> _serializer;
        public HttpContextFactory(IServiceProvider serviceProvider, ISerializer<string> serializer)
        {
            _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
        public OwinContext Create(object feature)
        {
            HttpContext httpContext = (feature as HttpContext) ?? throw new InvalidOperationException("当前上线文信息为null或类型错误");
            if (!MediaTypeHeaderValue.TryParse(httpContext.Request.ContentType, out var mediaTypeHeader)
                || !string.Equals("application/json", mediaTypeHeader?.MediaType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"Invalid 'Content-Type' header: value '{httpContext.Request.ContentType}' could not be parsed.");
            }
            else
            {
                using (StreamReader streamReader = new StreamReader(httpContext.Request.Body))
                {
                    var formData = streamReader.ReadToEnd();
                    return new OwinContext(_serializer.Deserialize<OwinRequest>(formData))
                    {
                        RequestServices = _serviceScopeFactory.CreateScope().ServiceProvider,
                    };
                }
            }
        }

        public void Dispose(OwinContext context)
        {
            context = null;
        }
    }
}
