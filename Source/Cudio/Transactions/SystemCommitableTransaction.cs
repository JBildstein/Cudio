using System.Threading.Tasks;
using System.Transactions;

namespace Cudio
{
    /// <summary>
    /// Wrapper around a <see cref="CommittableTransaction"/>.
    /// </summary>
    public sealed class SystemCommitableTransaction : ITransaction
    {
        private readonly CommittableTransaction transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemCommitableTransaction"/> class.
        /// </summary>
        /// <param name="transaction">The underlying transaction.</param>
        public SystemCommitableTransaction(CommittableTransaction transaction)
        {
            this.transaction = transaction;
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
            await Task.Factory.FromAsync(transaction.BeginCommit, transaction.EndCommit, null);
        }

        /// <inheritdoc/>
        public Task Rollback()
        {
            transaction.Rollback();
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            transaction.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
