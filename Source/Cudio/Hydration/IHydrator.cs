using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Contains methods to hydrate a read side table.
    /// </summary>
    public interface IHydrator
    {
        /// <summary>
        /// Hydrates a read side table.
        /// </summary>
        /// <returns>A <see cref="Task"/> for the operation.</returns>
        Task Hydrate();
    }
}
