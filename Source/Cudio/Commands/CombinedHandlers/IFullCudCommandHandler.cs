namespace Cudio
{
    /// <summary>
    /// Provides methods to authorize, validate and execute a CUD command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IFullCudCommandHandler<T> :
        ICommandHandler<CudCommand<T>>,
        ICommandAuthorizer<CudCommand<T>>,
        ICommandValidator<CudCommand<T>>
    {
    }
}
