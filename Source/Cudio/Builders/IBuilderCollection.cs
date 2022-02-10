using System.Collections.Generic;

namespace Cudio
{
    /// <summary>
    /// A collection of read table builders.
    /// </summary>
    public interface IBuilderCollection
    {
        /// <summary>
        /// Gets the handlers of a certain change type for the defined model type.
        /// </summary>
        /// <typeparam name="T">The type of the model that changed.</typeparam>
        /// <param name="changeType">The type of change that needs to be handled.</param>
        /// <returns>The list of handler infos for the given change type and model type.</returns>
        IEnumerable<HandlerInfo<T>> GetHandlersFor<T>(ChangeType changeType);
    }
}
