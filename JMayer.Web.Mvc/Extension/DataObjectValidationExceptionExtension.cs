using JMayer.Data.Database.DataLayer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JMayer.Web.Mvc.Extension;

/// <summary>
/// The static class has methods that extend the functionality of the DataObjectValidationException class.
/// </summary>
public static class DataObjectValidationExceptionExtension
{
    /// <summary>
    /// The method copies the validation results in the exception to the model state.
    /// </summary>
    /// <param name="exception">The exception to copy.</param>
    /// <param name="modelState">The model state to receive the validation results.</param>
    public static void CopyToModelState(this DataObjectValidationException exception, ModelStateDictionary modelState)
    {
        foreach (var result in exception.ValidationResults)
        {
            if (result.ErrorMessage is not null)
            {
                foreach (var memberName in result.MemberNames)
                {
                    modelState.AddModelError(memberName, result.ErrorMessage);
                }
            }
        }
    }
}
