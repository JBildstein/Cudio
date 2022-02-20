using System.Security.Claims;

namespace Cudio
{
    public sealed class DummyClaimsPrincipalProvider : IClaimsPrincipalProvider
    {
        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return new ClaimsPrincipal();
        }
    }
}
