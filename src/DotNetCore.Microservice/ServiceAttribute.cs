using System;

namespace DotNetCore.Microservice
{

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ServiceAttribute : Attribute
    {
        /// <summary>
        /// 服务组
        /// </summary>
        public string Group { get; set; }
    }
}
