using System;

namespace Cudio
{
    /// <summary>
    /// Interface for a command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the ID of this command instance.
        /// </summary>
        Guid CommandId { get; }
    }
}
