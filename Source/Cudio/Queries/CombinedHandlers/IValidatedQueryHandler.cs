namespace Cudio
{
    /// <summary>
    /// Provides methods to validate and execute a query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public interface IValidatedQueryHandler<TQuery, TResult> :
        IQueryHandler<TQuery, TResult>,
        IQueryValidator<TQuery>
        where TQuery : IQuery<TResult>
    {
    }
}
