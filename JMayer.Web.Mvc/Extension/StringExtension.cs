using System.Text.RegularExpressions;

namespace JMayer.Web.Mvc.Extension;

/// <summary>
/// The static class has methods that extend the functionality of the string class.
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// The method spaces the words in a camel case string.
    /// </summary>
    /// <param name="value">The string to modify.</param>
    /// <returns>The new string.</returns>
    public static string SpaceCamelCase(this string value)
    {
        return Regex.Replace(value, "([A-Z])", " $1").Trim();
    }
}
