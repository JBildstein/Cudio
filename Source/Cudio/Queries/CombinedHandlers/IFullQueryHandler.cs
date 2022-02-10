namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize, validate and execute a query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public interface IFullQueryHandler<TQuery, TResult> :
        IQueryHandler<TQuery, TResult>,
        IQueryAuthorizer<TQuery>,
        IQueryValidator<TQuery>
        where TQuery : IQuery<TResult>
    {
    }
}
