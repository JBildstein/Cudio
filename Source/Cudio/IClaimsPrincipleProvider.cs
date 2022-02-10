using System.Security.Claims;

namespace Cudio
{
    /// <summary>
    /// Provider for a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    public interface IClaimsPrincipleProvider
    {
        /// <summary>
        /// Gets the current <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <returns>The current <see cref="ClaimsPrincipal"/>.</returns>
        ClaimsPrincipal GetClaimsPrincipal();
    }
}
