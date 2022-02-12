using System.Data.Common;
using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Wrapper around a <see cref="DbTransaction"/>.
    /// </summary>
    public sealed class SystemDataTransaction : ITransaction
    {
        /// <summary>
        /// Gets the underlying transaction.
        /// </summary>
        public DbTransaction Transaction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemDataTransaction"/> class.
        /// </summary>
        /// <param name="transaction">The underlying transaction.</param>
        public SystemDataTransaction(DbTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// Explicit cast from <see cref="DbTransaction"/> to <see cref="SystemDataTransaction"/>.
        /// </summary>
        /// <param name="transaction"></param>
        public static explicit operator SystemDataTransaction(DbTransaction transaction)
        {
            return new SystemDataTransaction(transaction);
        }

        /// <inheritdoc/>
        public async Task Commit()
        {
            await Transaction.CommitAsync();
        }

        /// <inheritdoc/>
        public async Task Rollback()
        {
            await Transaction.RollbackAsync();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await Transaction.DisposeAsync();
        }
    }
}
