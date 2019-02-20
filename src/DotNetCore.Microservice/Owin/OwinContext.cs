using System;
using System.Collections.Generic;

namespace DotNetCore.Microservice.Owin
{
    public class OwinContext
    {
        public OwinContext(OwinRequest request)
        {
            Request = request;
            Response = new OwinResponse(request.Id);
        }

        /// <summary>
        /// 提供对当前请求级别容器的服务的访问
        /// </summary>
        public IServiceProvider RequestServices { get; set; }

        /// <summary>
        /// 当前请求信息
        /// </summary>
        public OwinRequest Request { get; }

        /// <summary>
        /// 当前响应信息
        /// </summary>
        public OwinResponse Response { get; }

        /// <summary>
        /// 当前上下文拓展信息
        /// </summary>
        public IDictionary<string, object> Items { get; private set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 获取拓展信息内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return Items.TryGetValue(key, out object value) ? (T)value : default(T);
        }

        /// <summary>
        /// 设置拓展信息内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public OwinContext Set<T>(string key, T value)
        {
            Items[key] = value;
            return this;
        }
    }
}
