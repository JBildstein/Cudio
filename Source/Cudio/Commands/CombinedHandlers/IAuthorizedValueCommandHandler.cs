namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize and execute a value command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IAuthorizedValueCommandHandler<T> :
        ICommandHandler<ValueCommand<T>>,
        ICommandAuthorizer<ValueCommand<T>>
    {
    }
}
