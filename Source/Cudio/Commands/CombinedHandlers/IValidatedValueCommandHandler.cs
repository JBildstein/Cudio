namespace Cudio
{
    /// <summary>
    /// Provides methods to validate and execute a value command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IValidatedValueCommandHandler<T> :
        ICommandHandler<ValueCommand<T>>,
        ICommandValidator<ValueCommand<T>>
    {
    }
}
