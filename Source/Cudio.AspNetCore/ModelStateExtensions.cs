using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Cudio.AspNetCore
{
    /// <summary>
    /// Contains extension methods for the MVC model state.
    /// </summary>
    public static class ModelStateExtensions
    {
        /// <summary>
        /// Applies errors from the validation context to the model state.
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="modelState">The model state.</param>
        public static void ToModelState(this ValidationContext validationContext, ModelStateDictionary modelState)
        {
            foreach (var errorList in validationContext.Errors)
            {
                foreach (string error in errorList.Value)
                {
                    modelState.AddModelError(errorList.Key, error);
                }
            }
        }
    }
}
