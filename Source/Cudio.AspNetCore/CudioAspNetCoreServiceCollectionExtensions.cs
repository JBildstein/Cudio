using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cudio.AspNetCore
{
    /// <summary>
    /// Contains methods to register Cudio related services, commands and query handlers for use with ASP.NET Core.
    /// </summary>
    public static class CudioAspNetCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the Cudio services and registers command and query handlers defined in the calling assembly.
        /// You still need to add a <see cref="IServiceScopeFactory"/> yourself depending on what DB or ORM you use.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddCudio(this IServiceCollection services)
        {
            return AddCudioBase(services, new CudioServiceDiscovery());
        }

        /// <summary>
        /// Adds the Cudio services and registers command and query handlers defined in the given assembly.
        /// You still need to add a <see cref="IServiceScopeFactory"/> yourself depending on what DB or ORM you use.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="handlerAssembly">The assembly that contains command and query handler types.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddCudio(this IServiceCollection services, Assembly handlerAssembly)
        {
            return AddCudioBase(services, new CudioServiceDiscovery(handlerAssembly));
        }

        /// <summary>
        /// Adds the Cudio services and registers command and query handlers defined in the given assemblies.
        /// You still need to add a <see cref="IServiceScopeFactory"/> yourself depending on what DB or ORM you use.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="handlerAssemblies">The assemblies that contain command and query handler types.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddCudio(this IServiceCollection services, params Assembly[] handlerAssemblies)
        {
            return AddCudioBase(services, new CudioServiceDiscovery(handlerAssemblies));
        }

        /// <summary>
        /// Adds the Cudio services and registers command and query handlers defined in the given assembly.
        /// You still need to add a <see cref="IServiceScopeFactory"/> yourself depending on what DB or ORM you use.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="allowedTypes">The list of types that contains various handlers for Cudio.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddCudio(this IServiceCollection services, IEnumerable<TypeInfo> allowedTypes)
        {
            return AddCudioBase(services, new CudioServiceDiscovery(allowedTypes));
        }

        private static IServiceCollection AddCudioBase(IServiceCollection services, CudioServiceDiscovery discovery)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IClaimsPrincipalProvider, HttpContextClaimsPrincipalProvider>();
            discovery.AddAll(services);

            return services;
        }
    }
}
