using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize a command.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public interface ICommandAuthorizer<T>
        where T : ICommand
    {
        /// <summary>
        /// Authorizes a command.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="command">The command to authorize.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Authorize(AuthorizationContext context, T command);
    }
}
