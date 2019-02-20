using System;
using System.Collections.Generic;

namespace DotNetCore.Microservice.Owin
{
    public class OwinResponse
    {
        public OwinResponse(string requestId)
        {
            RequestId = requestId;
        }

        /// <summary>
        /// 请求传来的唯一标识
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
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
        public void Success(object result)
        {
            this.StatusCode = 200;
            this.StatusMessage = "success";
            this.ReturnValue = result;
        }

        /// <summary>
        /// 404
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void Nofound(string msg = "")
        {
            this.StatusCode = 404;
            this.StatusMessage = string.IsNullOrWhiteSpace(msg) ? "Not Found" : msg;
            this.ReturnValue = "";
        }

        /// <summary>
        /// 500
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void Error(string msg = null)
        {
            this.StatusCode = 500;
            this.StatusMessage = string.IsNullOrWhiteSpace(msg) ? "Internal Server Error" : msg;
            this.ReturnValue = "";
        }

        /// <summary>
        /// 401
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void Unauthorized(string msg = null)
        {
            this.StatusCode = 401;
            this.StatusMessage = string.IsNullOrWhiteSpace(msg) ? "Unauthorized" : msg;
            this.ReturnValue = "";
        }
    }
}
