namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize, validate and execute a value command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IFullValueCommandHandler<T> :
        ICommandHandler<ValueCommand<T>>,
        ICommandAuthorizer<ValueCommand<T>>,
        ICommandValidator<ValueCommand<T>>
    {
    }
}
