using System.Security.Claims;

namespace Cudio
{
    public sealed class DummyClaimsPrincipleProvider : IClaimsPrincipleProvider
    {
        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return new ClaimsPrincipal();
        }
    }
}
