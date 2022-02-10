using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Context for executing a command.
    /// </summary>
    public abstract class ExecutionContext
    {
        /// <summary>
        /// Registers a newly created model that should cause a read side update.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="value">The created model.</param>
        public void RegisterCreate<T>(T value)
            where T : class
        {
            RegisterChange(default, value, ChangeType.Create);
        }

        /// <summary>
        /// Registers an updated model that should cause a read side update.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="oldValue">The previous model.</param>
        /// <param name="newValue">The changed model.</param>
        public void RegisterUpdate<T>(T oldValue, T newValue)
            where T : class
        {
            RegisterChange(oldValue, newValue, ChangeType.Update);
        }

        /// <summary>
        /// Registers a deleted model that should cause a read side update.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="value">The deleted model.</param>
        public void RegisterDelete<T>(T value)
            where T : class
        {
            RegisterChange(default, value, ChangeType.Delete);
        }

        /// <summary>
        /// Executes another command within this context.
        /// Authorization and validation will NOT be executed.
        /// </summary>
        /// <typeparam name="T">The type of the subcommand.</typeparam>
        /// <param name="command">The subcommand to execute.</param>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        public abstract Task ExecuteSubcommand<T>(T command) where T : ICommand;

        /// <summary>
        /// Registers a change in a model that should cause a read side update.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="oldValue">The previous model.</param>
        /// <param name="newValue">The changed model.</param>
        /// <param name="changeType">The type of the change.</param>
        protected abstract void RegisterChange<T>(T? oldValue, T newValue, ChangeType changeType)
            where T : class;
    }
}
