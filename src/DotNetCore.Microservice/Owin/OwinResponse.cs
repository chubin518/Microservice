using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCore.Microservice.Owin
{
   public class OwinResponse
    {/// <summary>
     /// 响应值
     /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 响应描述
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 拓展信息
        /// </summary>
        public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 200
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static OwinResponse Success(object result)
        {
            return new OwinResponse()
            {
                StatusCode = 200,
                StatusMessage = "success",
                ReturnValue = result
            };
        }

        /// <summary>
        /// 404
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static OwinResponse Nofound(string msg = "")
        {
            return new OwinResponse()
            {
                StatusCode = 404,
                StatusMessage = string.IsNullOrWhiteSpace(msg) ? "Not Found" : msg,
                ReturnValue = ""
            };
        }

        /// <summary>
        /// 500
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static OwinResponse Error(string msg = null)
        {
            return new OwinResponse()
            {
                StatusCode = 500,
                StatusMessage = string.IsNullOrWhiteSpace(msg) ? "Internal Server Error" : msg,
                ReturnValue = ""
            };
        }

        /// <summary>
        /// 401
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static OwinResponse Unauthorized(string msg = null)
        {
            return new OwinResponse()
            {
                StatusCode = 401,
                StatusMessage = string.IsNullOrWhiteSpace(msg) ? "Unauthorized" : msg,
                ReturnValue = ""
            };
        }
    }
}
