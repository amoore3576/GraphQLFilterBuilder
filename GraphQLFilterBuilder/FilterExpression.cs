namespace GraphQLFilterBuilder;

/// <summary>
/// Represents a filter expression that can be serialized to GraphQL arguments
/// </summary>
public class FilterExpression
{
    internal Dictionary<string, object?> Filters { get; } = new();
    internal List<FilterExpression> AndConditions { get; } = new();
    internal List<FilterExpression> OrConditions { get; } = new();

    /// <summary>
    /// Converts the filter expression to a dictionary suitable for GraphQL query variables
    /// </summary>
    public Dictionary<string, object?> ToDictionary()
    {
        var result = new Dictionary<string, object?>();

        // Add direct filters
        foreach (var filter in Filters)
        {
            result[filter.Key] = filter.Value;
        }

        // Add AND conditions
        if (AndConditions.Count > 0)
        {
            result["and"] = AndConditions.Select(c => c.ToDictionary()).ToArray();
        }

        // Add OR conditions
        if (OrConditions.Count > 0)
        {
            result["or"] = OrConditions.Select(c => c.ToDictionary()).ToArray();
        }

        return result;
    }
}