using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Cudio.AspNetCore
{
    /// <summary>
    /// Provider for a <see cref="ClaimsPrincipal"/> from the HTTP context.
    /// </summary>
    public class HttpContextClaimsPrincipalProvider : IClaimsPrincipleProvider
    {
        private static readonly ClaimsPrincipal DefaultPrincipal = new();

        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextClaimsPrincipalProvider"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public HttpContextClaimsPrincipalProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc/>
        public ClaimsPrincipal GetClaimsPrincipal()
        {
            return httpContextAccessor.HttpContext?.User ?? DefaultPrincipal;
        }
    }
}
