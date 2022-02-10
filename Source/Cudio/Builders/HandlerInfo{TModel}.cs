using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Contains infos about a read table builder handler.
    /// </summary>
    public abstract class HandlerInfo<TModel> : HandlerInfo
    {
        /// <summary>
        /// Handles changes for a certain type (e.g. Create).
        /// </summary>
        /// <param name="serviceProvider">Service provider for DI.</param>
        /// <param name="changes">The changes to handle.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        public abstract Task Handle(IServiceProvider serviceProvider, IEnumerable<Change<TModel>> changes);
    }
}
