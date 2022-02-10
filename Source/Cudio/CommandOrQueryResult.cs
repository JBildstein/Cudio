namespace Cudio
{
    /// <summary>
    /// Contains the result of a command or query.
    /// </summary>
    public abstract class CommandOrQueryResult
    {
        /// <summary>
        /// Gets the validation context.
        /// </summary>
        public ValidationContext Validation { get; }

        /// <summary>
        /// Gets the authorization context.
        /// </summary>
        public AuthorizationContext Authorization { get; }

        /// <summary>
        /// Gets a value indicating whether or not the execution was successfully.
        /// </summary>
        public bool Successful
        {
            get { return !Unauthorized && !ValidationFailed; }
        }

        /// <summary>
        /// Gets a value indicating whether or not authorization has failed.
        /// </summary>
        public bool Unauthorized
        {
            get { return !Authorization.HasSucceeded; }
        }

        /// <summary>
        /// Gets a value indicating whether or not validation has failed.
        /// </summary>
        public bool ValidationFailed
        {
            get { return Validation.HasErrors; }
        }

        private protected CommandOrQueryResult(
            ValidationContext validation,
            AuthorizationContext authorization)
        {
            Validation = validation;
            Authorization = authorization;
        }
    }
}
