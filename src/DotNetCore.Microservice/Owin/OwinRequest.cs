using System;
using System.Collections.Generic;

namespace DotNetCore.Microservice.Owin
{
    public class OwinRequest
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 服务组
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// 路由地址
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public object[] Parameters { get; set; } = Array.Empty<object>();

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout { get; set; } = 3000;

        /// <summary>
        /// 当前请求内容拓展信息
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
        public OwinRequest Set<T>(string key, T value)
        {
            Items[key] = value;
            return this;
        }
    }
}
