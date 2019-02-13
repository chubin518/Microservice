using System;
using System.Linq;
using System.Reflection;

namespace DotNetCore.Microservice.Routing
{
    public class RoutePath
    {
        public static string Parse(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            return method.DeclaringType.FullName + "." + method.Name + ":" + string.Join("_", method.GetParameters().Select(item => item.Name));
        }
    }
}
