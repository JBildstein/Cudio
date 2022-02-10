namespace Cudio
{
    /// <summary>
    /// Provides methods to validate and execute a CUD command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface IValidatedCudCommandHandler<T> :
        ICommandHandler<CudCommand<T>>,
        ICommandValidator<CudCommand<T>>
    {
    }
}
