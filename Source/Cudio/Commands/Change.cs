namespace Cudio
{
    /// <summary>
    /// Represents a change of value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Change<T>
    {
        /// <summary>
        /// The old value (if available).
        /// </summary>
        public readonly T? OldValue;

        /// <summary>
        /// The new value.
        /// </summary>
        public readonly T NewValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Change{T}"/> struct.
        /// </summary>
        /// <param name="oldValue">The old value (if available).</param>
        /// <param name="newValue">The new value.</param>
        public Change(T? oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
