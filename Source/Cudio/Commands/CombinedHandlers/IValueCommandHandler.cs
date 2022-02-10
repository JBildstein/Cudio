namespace Cudio
{
    /// <summary>
    /// Provides methods to execute a value command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IValueCommandHandler<T> : ICommandHandler<ValueCommand<T>>
    {
    }
}
