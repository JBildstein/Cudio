namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize and execute a CUD command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IAuthorizedCudCommandHandler<T> :
        ICommandHandler<CudCommand<T>>,
        ICommandAuthorizer<CudCommand<T>>
    {
    }
}
