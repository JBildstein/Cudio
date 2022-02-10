using System;
using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Represents a database transaction.
    /// </summary>
    public interface ITransaction : IAsyncDisposable
    {
        /// <summary>
        /// Attempts to commit the transaction
        /// </summary>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Commit();

        /// <summary>
        /// Rolls back (aborts) the transaction.
        /// </summary>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Rollback();
    }
}
