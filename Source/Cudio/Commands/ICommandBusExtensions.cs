using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Contains extension methods for the <see cref="ICommandBus"/>.
    /// </summary>
    public static class ICommandBusExtensions
    {
        /// <summary>
        /// Executes a <see cref="ValueCommand{T}"/> with the appropriate handler and does authorization and validation checks if available.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="bus">The bus to execute the command on.</param>
        /// <param name="value">The value to execute the command with.</param>
        /// <returns>The result of the command.</returns>
        public static async Task<CommandResult> ExecuteForValue<T>(this ICommandBus bus, T value)
        {
            return await bus.Execute(new ValueCommand<T>(value));
        }

        /// <summary>
        /// Executes a <see cref="CudCommand{T}"/> with the appropriate handler and does authorization and validation checks if available.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="bus">The bus to execute the command on.</param>
        /// <param name="value">The value to execute the command with.</param>
        /// <param name="changeType">The type of change for the value.</param>
        /// <returns>The result of the command.</returns>
        public static async Task<CommandResult> ExecuteForCud<T>(this ICommandBus bus, T value, ChangeType changeType)
        {
            return await bus.Execute(new CudCommand<T>(value, changeType));
        }
    }
}
