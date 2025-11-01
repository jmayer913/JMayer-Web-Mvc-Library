using System.Text.RegularExpressions;

namespace JMayer.Web.Mvc.Extension;

/// <summary>
/// The static class has methods that extend the functionality of the string class.
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// The method spaces capital letters in the string; the expectation is the capital letter is part of a word.
    /// </summary>
    /// <param name="value">The string to modify.</param>
    /// <returns>The new string.</returns>
    public static string SpaceCapitalLetters(this string value) => Regex.Replace(value, "([A-Z])", " $1").Trim();
}
