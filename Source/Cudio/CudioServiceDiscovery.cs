using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cudio
{
    /// <summary>
    /// Contains methods to register Cudio related services, commands and query handlers with DI.
    /// </summary>
    public class CudioServiceDiscovery
    {
        private readonly List<TypeInfo> types;

        /// <summary>
        /// Initializes a new instance of the <see cref="CudioServiceDiscovery"/> class.
        /// </summary>
        /// <remarks>The calling assembly is used to find types for commands, queries, handlers, etc.</remarks>
        public CudioServiceDiscovery()
            : this(Assembly.GetCallingAssembly())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CudioServiceDiscovery"/> class.
        /// </summary>
        /// <param name="handlerAssembly">The assembly that contains types for commands, queries, handlers, etc.</param>
        public CudioServiceDiscovery(Assembly handlerAssembly)
            : this(handlerAssembly.DefinedTypes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CudioServiceDiscovery"/> class.
        /// </summary>
        /// <param name="handlerAssemblies">The assemblies that contain contains types for commands, queries, handlers, etc.</param>
        public CudioServiceDiscovery(params Assembly[] handlerAssemblies)
            : this(handlerAssemblies.SelectMany(t => t.DefinedTypes))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CudioServiceDiscovery"/> class.
        /// </summary>
        /// <param name="allowedTypes">The list of types that contains commands, queries, handlers, etc.</param>
        public CudioServiceDiscovery(IEnumerable<TypeInfo> allowedTypes)
        {
            types = allowedTypes
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();
        }

        /// <summary>
        /// Adds the Cudio services and registers command and query handlers defined in the given assembly.
        /// You still need to add a <see cref="IClaimsPrincipalProvider"/> and <see cref="ITransactionFactory"/> yourself.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public void AddAll(IServiceCollection services)
        {
            AddBus(services);
            AddCommandHandlers(services);
            AddQueryHandlers(services);
            AddReadModelBuilders(services);
            AddReadModelHydrators(services);
        }

        /// <summary>
        /// Adds the command and query bus.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public void AddBus(IServiceCollection services)
        {
            services.AddScoped<ICommandBus, CommandBus>();
            services.AddScoped<IQueryBus, QueryBus>();
        }

        /// <summary>
        /// Adds the command handlers.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public void AddCommandHandlers(IServiceCollection services)
        {
            var commandHandlers = GetTypesWithGenericInterface(types, typeof(ICommandHandler<>));
            AddCommandHandlers(services, commandHandlers, "handler");

            var commandAuthorizers = GetTypesWithGenericInterface(types, typeof(ICommandAuthorizer<>));
            AddCommandHandlers(services, commandAuthorizers, "authorizer");

            var commandValidators = GetTypesWithGenericInterface(types, typeof(ICommandValidator<>));
            AddCommandHandlers(services, commandValidators, "validator");
        }

        /// <summary>
        /// Adds the query handlers.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public void AddQueryHandlers(IServiceCollection services)
        {
            var queries = GetTypesWithGenericInterface(types, typeof(IQuery<>));
            foreach (var query in queries)
            {
                var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.Type, query.TypeArgument);
                AddQueryHandlers(services, types, query.Type, handlerType, "handler", optional: false);

                var authorizerType = typeof(IQueryAuthorizer<>).MakeGenericType(query.Type);
                AddQueryHandlers(services, types, query.Type, authorizerType, "authorizer", optional: true);

                var validatorType = typeof(IQueryValidator<>).MakeGenericType(query.Type);
                AddQueryHandlers(services, types, query.Type, validatorType, "validator", optional: true);
            }
        }

        /// <summary>
        /// Adds the read model builders.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public void AddReadModelBuilders(IServiceCollection services)
        {
            var builders = GetTypesWithGenericInterface(types, typeof(IReadModelBuilder<>));
            var duplicateBuilders = builders.GroupBy(t => t.TypeArgument)
                .Where(t => t.Skip(1).Any())
                .ToList();
            if (duplicateBuilders.Count != 0)
            {
                string message = "Duplicate read model builders:";
                foreach (var builder in duplicateBuilders)
                {
                    message += $"{Environment.NewLine}  Builders for \"{builder.Key.Name}\": {string.Join(", ", builder.Select(t => t.Type.Name))}";
                }

                throw new AmbiguousMatchException(message);
            }

            services.AddSingleton<IBuilderCollection>(new BuilderCollection(builders.Select(t => t.Type)));
            foreach (var builder in builders) { services.AddScoped(builder.Type); }
        }

        /// <summary>
        /// Adds the read model hydrators.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        public void AddReadModelHydrators(IServiceCollection services)
        {
            var hydrators = GetTypesWithGenericInterface(types, typeof(IHydrator<>));
            var duplicateHydrators = hydrators.GroupBy(t => t.TypeArgument)
                .Where(t => t.Skip(1).Any())
                .ToList();
            if (duplicateHydrators.Count != 0)
            {
                string message = "Duplicate hydrators:";
                foreach (var hydrator in duplicateHydrators)
                {
                    message += $"{Environment.NewLine}  Hydrator for \"{hydrator.Key.Name}\": {string.Join(", ", hydrator.Select(t => t.Type.Name))}";
                }

                throw new AmbiguousMatchException(message);
            }

            foreach (var hydrator in hydrators)
            {
                services.AddScoped(typeof(IHydrator), hydrator.Type);
                services.AddScoped(hydrator.InterfaceType, hydrator.Type);
            }
        }

        private static IEnumerable<TypeInfo> GetTypesWithInterface(IEnumerable<TypeInfo> types, Type @interface)
        {
            return types.Where(t => t.GetInterfaces().Contains(@interface));
        }

        private static IEnumerable<TypeInterfaceInfo> GetTypesWithGenericInterface(IEnumerable<TypeInfo> types, Type @interface)
        {
            foreach (var type in types)
            {
                var interfaceType = type.GetInterfaces()
                    .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == @interface)
                    .FirstOrDefault();
                if (interfaceType != null) { yield return new TypeInterfaceInfo(type, interfaceType, interfaceType.GenericTypeArguments[0]); }
            }
        }

        private static void AddCommandHandlers(IServiceCollection services, IEnumerable<TypeInterfaceInfo> types, string handlerName)
        {
            var usedTypes = new HashSet<Type>();
            foreach (var type in types)
            {
                var commandType = type.InterfaceType.GenericTypeArguments[0];
                if (usedTypes.Contains(commandType))
                {
                    string name;
                    if (commandType.IsGenericType && commandType.GetGenericTypeDefinition() == typeof(ValueCommand<>))
                    {
                        name = "value command of " + commandType.GenericTypeArguments[0].Name;
                    }
                    else if (commandType.IsGenericType && commandType.GetGenericTypeDefinition() == typeof(CudCommand<>))
                    {
                        name = "CUD command of " + commandType.GenericTypeArguments[0].Name;
                    }
                    else { name = "command " + commandType.Name; }

                    throw new AmbiguousMatchException($"Multiple {handlerName}s found for {name}");
                }

                usedTypes.Add(commandType);
                services.AddScoped(type.InterfaceType, type.Type);
            }
        }

        private static void AddQueryHandlers(IServiceCollection services, IEnumerable<TypeInfo> types, Type item, Type handlerType, string handlerName, bool optional)
        {
            var handlers = GetTypesWithInterface(types, handlerType).ToList();
            if (handlers.Count == 1) { services.AddScoped(handlerType, handlers[0]); }
            else if (handlers.Count > 1) { throw new AmbiguousMatchException($"Multiple {handlerName}s found for query {item.Name}"); }
            else if (!optional) { throw new InvalidOperationException($"No {handlerName} found for query {item.Name}"); }
        }

        private readonly record struct TypeInterfaceInfo(TypeInfo Type, Type InterfaceType, Type TypeArgument);
    }
}
