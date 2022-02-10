using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Provides methods to validate a command.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public interface ICommandValidator<T>
        where T : ICommand
    {
        /// <summary>
        /// Validates a command.
        /// </summary>
        /// <param name="context">The validation context.</param>
        /// <param name="command">The command to validate.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Validate(ValidationContext context, T command);
    }
}
