namespace GraphQLFilterBuilder;

using System.Text.Json;

/// <summary>
/// Builds GraphQL filters dynamically from key-value pairs
/// </summary>
public class DynamicFilterBuilder
{
    private readonly GraphQLFilterOptions _options;

    public DynamicFilterBuilder(GraphQLFilterOptions? options = null)
    {
        _options = options ?? GraphQLFilterOptions.Default;
    }

    /// <summary>
    /// Builds a filter from key-value pairs where key is "property.operator"
    /// Example: { "age.gt": 18, "status.eq": "Active", "name.contains": "John" }
    /// </summary>
    public Dictionary<string, object?> BuildFromKeyValuePairs(Dictionary<string, object?> filters)
    {
        var result = new Dictionary<string, object?>();

        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                continue;

            var parts = key.Split('.');
            if (parts.Length != 2)
                continue;

            var propertyName = parts[0];
            var operatorString = parts[1];

            if (!TryParseOperator(operatorString, out var op))
                continue;

            var filterKey = _options.UseCamelCase ? propertyName.ToCamelCase() : propertyName;
            var operatorKey = OperatorMapping.GetOperatorKey(op, _options.Convention);

            result[filterKey] = new Dictionary<string, object?>
            {
                [operatorKey] = OperatorMapping.FormatValue(op, value, _options.Convention)
            };
        }

        return result;
    }

    /// <summary>
    /// Builds a filter from a flat dictionary using default equality operator
    /// Example: { "age": 18, "status": "Active", "name": "John" }
    /// </summary>
    public Dictionary<string, object?> BuildFromSimpleKeyValuePairs(Dictionary<string, object?> filters)
    {
        var result = new Dictionary<string, object?>();

        foreach (var (key, value) in filters)
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                continue;

            var filterKey = _options.UseCamelCase ? key.ToCamelCase() : key;
            var operatorKey = OperatorMapping.GetOperatorKey(FilterOperator.Equal, _options.Convention);

            result[filterKey] = new Dictionary<string, object?>
            {
                [operatorKey] = value
            };
        }

        return result;
    }

    private bool TryParseOperator(string operatorString, out FilterOperator op)
    {
        op = operatorString.ToLowerInvariant() switch
        {
            "eq" or "equals" or "=" => FilterOperator.Equal,
            "neq" or "ne" or "notequals" or "!=" => FilterOperator.NotEqual,
            "gt" or "greaterthan" or ">" => FilterOperator.GreaterThan,
            "gte" or "ge" or "greaterthanorequal" or ">=" => FilterOperator.GreaterThanOrEqual,
            "lt" or "lessthan" or "<" => FilterOperator.LessThan,
            "lte" or "le" or "lessthanorequal" or "<=" => FilterOperator.LessThanOrEqual,
            "contains" or "like" => FilterOperator.Contains,
            "startswith" or "starts" => FilterOperator.StartsWith,
            "endswith" or "ends" => FilterOperator.EndsWith,
            "in" => FilterOperator.In,
            "nin" or "notin" => FilterOperator.NotIn,
            "isnull" or "null" => FilterOperator.IsNull,
            "isnotnull" or "notnull" => FilterOperator.IsNotNull,
            _ => default
        };

        return op != default;
    }
}