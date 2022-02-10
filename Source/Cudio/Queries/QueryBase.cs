using System;

namespace Cudio
{
    /// <summary>
    /// Base class for a query.
    /// </summary>
    /// <typeparam name="T">The type of the result of the query.</typeparam>
    public abstract class QueryBase<T> : IQuery<T>
    {
        /// <inheritdoc/>
        public Guid QueryId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBase{T}"/> class.
        /// </summary>
        protected QueryBase()
        {
            QueryId = Guid.NewGuid();
        }
    }
}
