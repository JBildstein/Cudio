using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Provides methods to execute a command.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public interface ICommandHandler<T>
        where T : ICommand
    {
        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Execute(ExecutionContext context, T command);
    }
}
