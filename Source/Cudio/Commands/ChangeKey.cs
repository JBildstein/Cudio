using System;

namespace Cudio
{
    internal readonly record struct ChangeKey(Type Type, ChangeType ChangeType)
    {
        public static ChangeKey For<T>(ChangeType changeType)
        {
            return new ChangeKey(typeof(T), changeType);
        }
    }
}
