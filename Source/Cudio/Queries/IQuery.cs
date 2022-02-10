using System;

namespace Cudio
{
    /// <summary>
    /// Interface for a query.
    /// </summary>
    public interface IQuery
    {
        /// <summary>
        /// Gets the ID of this query instance.
        /// </summary>
        Guid QueryId { get; }
    }
}
