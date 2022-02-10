using System;
using System.Diagnostics.CodeAnalysis;

namespace Cudio
{
    /// <summary>
    /// Contains the result of a query.
    /// </summary>
    public sealed class QueryResult<T> : CommandOrQueryResult
    {
        /// <summary>
        /// Gets the ID of the executed query.
        /// </summary>
        public Guid QueryId { get; }

        /// <summary>
        /// Gets the result value of the executed query.
        /// </summary>
        public T? Value { get; }

        internal QueryResult(
            IQuery query,
            T? value,
            ValidationContext validation,
            AuthorizationContext authorization)
            : base(validation, authorization)
        {
            QueryId = query.QueryId;
            Value = value;
        }
    }
}
