using System;
using System.Linq;
using System.Reflection;
using DevKit.Web.Services.Jwt;
using Microsoft.Extensions.DependencyInjection;

namespace DevKit.Web.Core
{
    
   public static class ServiceCollectionExtensions
    {
        public static void AddDevKitServices(this IServiceCollection services)
        {
            var assembly = Assembly.GetAssembly(typeof(IService));
            var types = assembly.GetExportedTypes().Where(t => t.IsClass && t.IsPublic && !t.IsAbstract);

            foreach (Type type in types)
            {
                foreach (Type iface in type.GetInterfaces())
                {
                    if (iface == typeof(IService))
                    {
                        services.AddScoped(iface, type);
                    }
                }
            }
        }
    }
}
