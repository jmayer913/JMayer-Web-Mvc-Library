using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace JMayer.Web.Mvc.Extension;

/// <summary>
/// The static class has methods that extend the functionality of the ModelStateDictionary class.
/// </summary>
public static class ModelStateDictionaryExtension
{
    /// <summary>
    /// Used when serializing this data object.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// The method returns the json of the model state errors.
    /// </summary>
    /// <param name="modelStateDictionary">The model state dictionary to serialize.</param>
    /// <returns>Json of the errors.</returns>
    public static string ErrorsToJson(this ModelStateDictionary modelStateDictionary)
    {
        Dictionary<string, List<string>> dictionary = [];

        foreach (var keyValuePair in modelStateDictionary)
        {
            if (keyValuePair.Value.Errors.Count == 1)
            {
                dictionary.Add(keyValuePair.Key, [keyValuePair.Value.Errors[0].ErrorMessage]);
            }
            else
            {
                List<string> errors = [];

                foreach (var error in keyValuePair.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }

                dictionary.Add(keyValuePair.Key, errors);
            }
        }

        return JsonSerializer.Serialize(dictionary, _jsonSerializerOptions);
    }
}
