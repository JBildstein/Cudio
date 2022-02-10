using System;

namespace Cudio
{
    /// <summary>
    /// Base class for a command.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <inheritdoc/>
        public Guid CommandId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBase"/> class.
        /// </summary>
        protected CommandBase()
        {
            CommandId = Guid.NewGuid();
        }
    }
}
