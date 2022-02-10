namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize, validate and execute a command.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public interface IFullCommandHandler<T> :
        ICommandHandler<T>,
        ICommandAuthorizer<T>,
        ICommandValidator<T>
        where T : ICommand
    {
    }
}
