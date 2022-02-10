namespace Cudio
{
    /// <summary>
    /// Provides methods to validate and execute a command.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public interface IValidatedCommandHandler<T> :
        ICommandHandler<T>,
        ICommandValidator<T>
        where T : ICommand
    {
    }
}
