using System;

namespace Cudio
{
    /// <summary>
    /// Contains the result of a command.
    /// </summary>
    public sealed class CommandResult : CommandOrQueryResult
    {
        /// <summary>
        /// Gets the ID of the executed command.
        /// </summary>
        public Guid CommandId { get; }

        internal CommandResult(
            ICommand command,
            ValidationContext validation,
            AuthorizationContext authorization)
            : base(validation, authorization)
        {
            CommandId = command.CommandId;
        }
    }
}
