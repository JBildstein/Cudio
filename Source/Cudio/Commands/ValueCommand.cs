namespace Cudio
{
    /// <summary>
    /// Command that represents an unspecified action on a model.
    /// The type of action is typically defined in the value. e.g. BookCreateModel.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    public sealed class ValueCommand<T> : CommandBase
    {
        /// <summary>
        /// Gets the value of the command.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCommand{T}"/> class.
        /// </summary>
        /// <param name="value">The value of the command.</param>
        public ValueCommand(T value)
        {
            Value = value;
        }
    }
}
