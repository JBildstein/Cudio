using System.Threading.Tasks;
using System.Transactions;

namespace Cudio
{
    /// <summary>
    /// Wrapper around a <see cref="CommittableTransaction"/>.
    /// </summary>
    public sealed class SystemCommitableTransaction : ITransaction
    {
        /// <summary>
        /// Gets the underlying transaction.
        /// </summary>
        public CommittableTransaction Transaction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemCommitableTransaction"/> class.
        /// </summary>
        /// <param name="transaction">The underlying transaction.</param>
        public SystemCommitableTransaction(CommittableTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// Explicit cast from <see cref="CommittableTransaction"/> to <see cref="SystemCommitableTransaction"/>.
        /// </summary>
        /// <param name="transaction"></param>
        public static explicit operator SystemCommitableTransaction(CommittableTransaction transaction)
        {
            return new SystemCommitableTransaction(transaction);
        }

        /// <inheritdoc/>
        public async Task Commit()
        {
            await Task.Factory.FromAsync(Transaction.BeginCommit, Transaction.EndCommit, null);
        }

        /// <inheritdoc/>
        public Task Rollback()
        {
            Transaction.Rollback();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            Transaction.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
