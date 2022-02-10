namespace Cudio
{
    /// <summary>
    /// Provides methods to execute a CUD command.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    public interface ICudCommandHandler<T> : ICommandHandler<CudCommand<T>>
    {
    }
}
