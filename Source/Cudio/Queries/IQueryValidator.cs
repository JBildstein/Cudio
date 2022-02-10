using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Provides methods to validate a query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public interface IQueryValidator<TQuery>
        where TQuery : IQuery
    {
        /// <summary>
        /// Validates a query.
        /// </summary>
        /// <param name="context">The validation context.</param>
        /// <param name="query">The query to validate.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Validate(ValidationContext context, TQuery query);
    }
}
