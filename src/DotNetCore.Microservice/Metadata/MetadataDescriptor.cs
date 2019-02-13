using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DotNetCore.Microservice.Metadata
{
    public class MetadataDescriptor
    {
        public MetadataDescriptor(TypeInfo service) : this(service, null)
        {

        }

        public MetadataDescriptor(TypeInfo service, TypeInfo implementation)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
            Implementation = implementation;
            Methods = service.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(item => item.ReturnType != typeof(void) && item.ReturnType != typeof(Task));
            ServiceAttribute attribute = service.GetCustomAttribute<ServiceAttribute>();
            Group = string.IsNullOrWhiteSpace(attribute.Group) ? service.Assembly.GetName().Name : attribute.Group;
        }

        public string Group { get; private set; }

        public TypeInfo Service { get; private set; }

        public TypeInfo Implementation { get; private set; }

        public IEnumerable<MethodInfo> Methods { get; private set; }

    }
}
