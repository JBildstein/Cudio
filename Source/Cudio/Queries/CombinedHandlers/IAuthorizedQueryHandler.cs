namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize and execute a query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    public interface IAuthorizedQueryHandler<TQuery, TResult> :
        IQueryHandler<TQuery, TResult>,
        IQueryAuthorizer<TQuery>
        where TQuery : IQuery<TResult>
    {
    }
}
