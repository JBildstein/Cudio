using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// A bus to handle queries.
    /// </summary>
    public interface IQueryBus
    {
        /// <summary>
        /// Executes the query with the appropriate handler and does authorization and validation checks if available.
        /// </summary>
        /// <typeparam name="T">The type of the result of the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>The result of the query.</returns>
        Task<QueryResult<T>> Execute<T>(IQuery<T> query);

        /// <summary>
        /// Executes the query with the appropriate handler but ignores authorization and validation checks.
        /// </summary>
        /// <typeparam name="T">The type of the result of the query.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>The result of the query.</returns>
        Task<T> ExecuteDirect<T>(IQuery<T> query);
    }
}
