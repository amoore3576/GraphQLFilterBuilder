namespace GraphQLFilterBuilder;

/// <summary>
/// String utility extensions for filter building
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}