using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Factory to create a transaction for read and write DBs.
    /// </summary>
    public interface ITransactionFactory
    {
        /// <summary>
        /// Opens a transaction for both the read and write DB.
        /// </summary>
        /// <returns>The transaction handler.</returns>
        Task<ITransaction> OpenTransaction();
    }
}
