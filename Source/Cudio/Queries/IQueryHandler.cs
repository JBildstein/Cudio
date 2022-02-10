using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Provides methods to execute a query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Executes a query.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>The result of the query.</returns>
        Task<TResult> Execute(TQuery query);
    }
}
