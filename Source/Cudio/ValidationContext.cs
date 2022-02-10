using System.Collections;
using System.Collections.Generic;

namespace Cudio
{
    /// <summary>
    /// Context for validating a command or query.
    /// </summary>
    public class ValidationContext
    {
        /// <summary>
        /// Gets a value indicating whether there are validation errors or not.
        /// </summary>
        public bool HasErrors
        {
            get { return errors.Count != 0; }
        }

        /// <summary>
        /// Gets the validation errors.
        /// </summary>
        public IReadOnlyDictionary<string, ValidationErrorCollection> Errors
        {
            get { return errors; }
        }

        private readonly Dictionary<string, ValidationErrorCollection> errors = new();

        /// <summary>
        /// Adds an error.
        /// </summary>
        /// <param name="key">The key of the error.</param>
        /// <param name="error">The error.</param>
        public void AddError(string key, string error)
        {
            if (!errors.TryGetValue(key, out var list))
            {
                list = new ValidationErrorCollection(key);
                errors.Add(key, list);
            }

            list.AddError(error);
        }

        /// <summary>
        /// Represents a list of validation errors for a key.
        /// </summary>
        public class ValidationErrorCollection : IEnumerable<string>
        {
            /// <summary>
            /// Gets the key of this error collection.
            /// </summary>
            public string Key { get; }

            private readonly List<string> errors = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="ValidationErrorCollection"/> class.
            /// </summary>
            /// <param name="key">The key of the errors.</param>
            internal ValidationErrorCollection(string key)
            {
                Key = key;
            }

            /// <inheritdoc/>
            public IEnumerator<string> GetEnumerator()
            {
                return errors.GetEnumerator();
            }

            /// <inheritdoc/>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal void AddError(string error)
            {
                errors.Add(error);
            }
        }
    }
}
