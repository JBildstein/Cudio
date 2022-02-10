namespace Cudio
{
    /// <summary>
    /// Command that represents a specific type of action on a model (Create, Update or Delete).
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public sealed class CudCommand<T> : CommandBase
    {
        /// <summary>
        /// Gets the value of the command.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the type of change for the <see cref="Value"/>.
        /// </summary>
        public ChangeType ChangeType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CudCommand{T}"/> class.
        /// </summary>
        /// <param name="value">The value of the command.</param>
        /// <param name="changeType">The type of change for the value.</param>
        public CudCommand(T value, ChangeType changeType)
        {
            Value = value;
            ChangeType = changeType;
        }
    }
}
