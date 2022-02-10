using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cudio
{
    /// <summary>
    /// A collection of read table builders.
    /// </summary>
    public class BuilderCollection : IBuilderCollection
    {
        private readonly Dictionary<ChangeKey, List<HandlerInfo>> handlers = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderCollection"/> class.
        /// </summary>
        /// <param name="builderTypes">The read table builder types to scan for handler methods.</param>
        public BuilderCollection(IEnumerable<Type> builderTypes)
        {
            foreach (var builderType in builderTypes)
            {
                var methods = builderType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    ChangeType? changeType = method.Name switch
                    {
                        "Create" => ChangeType.Create,
                        "Update" => ChangeType.Update,
                        "Delete" => ChangeType.Delete,
                        _ => null,
                    };

                    if (changeType == null) { continue; }

                    var parameters = method.GetParameters();
                    if (parameters.Length is not 1 and not 2) { continue; }
                    if (parameters.Length == 2 && parameters[0].ParameterType != parameters[1].ParameterType) { continue; }

                    var key = new ChangeKey(parameters[0].ParameterType, changeType.Value);
                    if (!handlers.TryGetValue(key, out var infos))
                    {
                        infos = new List<HandlerInfo>();
                        handlers.Add(key, infos);
                    }

                    infos.Add(HandlerInfo.From(builderType, key.Type, method));
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<HandlerInfo<T>> GetHandlersFor<T>(ChangeType changeType)
        {
            if (handlers.TryGetValue(ChangeKey.For<T>(changeType), out var infos)) { return infos.Cast<HandlerInfo<T>>(); }
            else { return Enumerable.Empty<HandlerInfo<T>>(); }
        }
    }
}
