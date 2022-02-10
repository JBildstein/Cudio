using System.Security.Claims;

namespace Cudio
{
    /// <summary>
    /// Context for authorizing a command or query.
    /// </summary>
    public class AuthorizationContext
    {
        /// <summary>
        /// Gets the user that wants to execute a command or query.
        /// </summary>
        public ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets a value indicating whether the authorization has succeeded.
        /// </summary>
        public bool HasSucceeded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the authorization has failed.
        /// </summary>
        public bool HasFailed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationContext"/> class.
        /// </summary>
        /// <param name="user">The user that wants to execute a command or query.</param>
        public AuthorizationContext(ClaimsPrincipal user)
        {
            User = user;
        }

        /// <summary>
        /// Marks the authorization as successful.
        /// If <see cref="Fail"/> has been called already, this has no effect.
        /// </summary>
        public void Succeed()
        {
            HasSucceeded = !HasFailed;
        }

        /// <summary>
        /// Marks the authorization as failed, even if <see cref="Succeed"/> has already been called.
        /// Subsequent calls to <see cref="Succeed"/> do not change the result anymore.
        /// </summary>
        public void Fail()
        {
            HasFailed = true;
            HasSucceeded = false;
        }
    }
}
