namespace Cudio
{
    /// <summary>
    /// Interface for a query.
    /// </summary>
    /// <typeparam name="T">The type of the result of the query.</typeparam>
    public interface IQuery<out T> : IQuery
    {
    }
}
