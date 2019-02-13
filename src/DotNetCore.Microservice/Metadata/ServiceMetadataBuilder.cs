
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotNetCore.Microservice.Metadata
{
    public class ServiceMetadataBuilder
    {
        private readonly List<Func<IEnumerable<Type>, IEnumerable<MetadataDescriptor>>> _scanners = new List<Func<IEnumerable<Type>, IEnumerable<MetadataDescriptor>>>();
        private readonly IHostingEnvironment _environment;
        private readonly IServiceCollection _services;
        public ServiceMetadataBuilder(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _environment = services
                .FirstOrDefault(item => item.ServiceType == typeof(IHostingEnvironment))
                ?.ImplementationInstance as IHostingEnvironment;

            _scanners.Add(AttributeServiceMetadatas);
        }

        public ServiceMetadataBuilder Scan(Func<IEnumerable<Type>, IEnumerable<MetadataDescriptor>> scan)
        {
            _scanners.Add(scan);
            return this;
        }

        public ServiceMetadata Build()
        {
            ServiceMetadata metadataCollection = new ServiceMetadata();
            var types = GetExportedTypes();
            foreach (var scanner in _scanners)
            {
                //先注入服务端实例
                foreach (var metadata in scanner(types).OrderByDescending(item => item.Implementation))
                {
                    if (metadata.Implementation == null)
                    {
                        _services.TryAddSingleton(metadata.Service, provier =>
                        {
                            return provier.GetRequiredService<IClientProxyFactory>().CreateProxy(metadata.Service);// 动态代理创建对应实例
                        });
                    }
                    else
                    {
                        _services.TryAddScoped(metadata.Service, metadata.Implementation);
                    }
                    metadataCollection.TryAdd(metadata);
                }
            }
            return metadataCollection;
        }

        /// <summary>
        /// 默认扫描器
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private IEnumerable<MetadataDescriptor> AttributeServiceMetadatas(IEnumerable<Type> types)
        {
            if (types == null)
            {
                return new List<MetadataDescriptor>();
            }
            return types.SelectMany(service =>
            {
                if (service.IsClass && !service.IsAbstract)
                {
                    return service
                     .GetInterfaces()
                     .Where(item => item.IsDefined(typeof(ServiceAttribute)))
                     .Select(item => new MetadataDescriptor(item.GetTypeInfo(), service.GetTypeInfo()));
                }
                if (service.IsInterface && service.IsDefined(typeof(ServiceAttribute)))
                {
                    return new List<MetadataDescriptor>() { new MetadataDescriptor(service.GetTypeInfo()) };
                }
                return new List<MetadataDescriptor>();
            });
        }



        private const string Ignore_Pattern = "^Microsoft.\\w*|^System.\\w*|^DotNetty.\\w*|^runtime.\\w*|^ZooKeeperNetEx\\w*|^StackExchange.Redis\\w*|^Consul\\w*|^Newtonsoft.\\w*|^Autofac.\\w*|^NETStandard.\\w*";
        private readonly static Regex Ignore_Library_Regex = new Regex(Ignore_Pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// 获取所有可能提供服务的对象
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Type> GetExportedTypes()
        {
            List<Assembly> assemblies = new List<Assembly>();
            Assembly entryAssembly;
            if (null != _environment && !string.IsNullOrWhiteSpace(_environment.ApplicationName))
            {
                entryAssembly = Assembly.Load(new AssemblyName(_environment.ApplicationName));
            }
            else
            {
                entryAssembly = Assembly.GetEntryAssembly();
            }
            var dependencies = DependencyContext.Load(entryAssembly).RuntimeLibraries;
            foreach (var library in dependencies)
            {
                if (!Ignore_Library_Regex.IsMatch(library.Name))
                {
                    var assembly = Assembly.Load(new AssemblyName(library.Name));
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }
                    assemblies.Add(assembly);
                }
            }
            return assemblies.SelectMany(item => item.GetExportedTypes());
        }
    }
}
