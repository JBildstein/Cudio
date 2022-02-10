namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize and execute a command.
    /// </summary>
    /// <typeparam name="T">The type of the command.</typeparam>
    public interface IAuthorizedCommandHandler<T> :
        ICommandHandler<T>,
        ICommandAuthorizer<T>
        where T : ICommand
    {
    }
}
