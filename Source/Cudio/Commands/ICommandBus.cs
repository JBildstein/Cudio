using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// A bus to handle commands.
    /// </summary>
    public interface ICommandBus
    {
        /// <summary>
        /// Executes the command with the appropriate handler and does authorization and validation checks if available.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>The result of the command.</returns>
        Task<CommandResult> Execute<TCommand>(TCommand command) where TCommand : ICommand;

        /// <summary>
        /// Executes the command with the appropriate handler but ignores authorization and validation checks.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task ExecuteDirect<TCommand>(TCommand command) where TCommand : ICommand;
    }
}
