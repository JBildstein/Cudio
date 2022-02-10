using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize a query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public interface IQueryAuthorizer<TQuery>
        where TQuery : IQuery
    {
        /// <summary>
        /// Authorizes a query.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="query">The query to authorize.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Authorize(AuthorizationContext context, TQuery query);
    }
}
