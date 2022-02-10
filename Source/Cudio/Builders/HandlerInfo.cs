using System;
using System.Reflection;

namespace Cudio
{
    /// <summary>
    /// Contains infos about a read table builder handler.
    /// </summary>
    public abstract class HandlerInfo
    {
        internal static HandlerInfo From(Type builderType, Type modelType, MethodInfo method)
        {
            var type = typeof(HandlerInfo<,>).MakeGenericType(builderType, modelType);
            return (HandlerInfo)Activator.CreateInstance(type, method)!;
        }
    }
}
