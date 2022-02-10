namespace Cudio
{
    /// <summary>
    /// Type of change to a model.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Newly created.
        /// </summary>
        Create,

        /// <summary>
        /// Existing updated.
        /// </summary>
        Update,

        /// <summary>
        /// Existing deleted.
        /// </summary>
        Delete,
    }
}
