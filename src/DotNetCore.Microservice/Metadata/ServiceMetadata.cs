using System.Collections.Generic;
using System.Linq;
namespace DotNetCore.Microservice.Metadata
{
    public class ServiceMetadata
    {
        private readonly List<MetadataDescriptor> _clients = new List<MetadataDescriptor>();
        private readonly List<MetadataDescriptor> _services = new List<MetadataDescriptor>();

        /// <summary>
        /// 客户端元数据
        /// </summary>
        public IReadOnlyCollection<MetadataDescriptor> Clients { get { return _clients; } }

        /// <summary>
        /// 服务端元数据
        /// </summary>
        public IReadOnlyCollection<MetadataDescriptor> Services { get { return _services; } }

        public void TryAdd(MetadataDescriptor metadata)
        {
            if (metadata.Implementation != null)
            {
                _services.Add(metadata);
            }
            else
            {
                if (!_services.Any(item => item.Service == metadata.Service))
                {
                    _clients.Add(metadata);
                }
            }
        }
    }
}
