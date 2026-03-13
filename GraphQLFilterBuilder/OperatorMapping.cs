namespace GraphQLFilterBuilder;

/// <summary>
/// Shared utility for mapping filter operators across different GraphQL conventions
/// </summary>
internal static class OperatorMapping
{
    /// <summary>
    /// Gets the operator key for the specified convention
    /// </summary>
    public static string GetOperatorKey(FilterOperator op, GraphQLConvention convention)
    {
        return convention switch
        {
            GraphQLConvention.HotChocolate => op switch
            {
                FilterOperator.Equal => "eq",
                FilterOperator.NotEqual => "neq",
                FilterOperator.GreaterThan => "gt",
                FilterOperator.GreaterThanOrEqual => "gte",
                FilterOperator.LessThan => "lt",
                FilterOperator.LessThanOrEqual => "lte",
                FilterOperator.Contains => "contains",
                FilterOperator.StartsWith => "startsWith",
                FilterOperator.EndsWith => "endsWith",
                FilterOperator.In => "in",
                FilterOperator.NotIn => "nin",
                FilterOperator.IsNull => "eq",
                FilterOperator.IsNotNull => "neq",
                _ => "eq"
            },
            GraphQLConvention.Hasura => op switch
            {
                FilterOperator.Equal => "_eq",
                FilterOperator.NotEqual => "_neq",
                FilterOperator.GreaterThan => "_gt",
                FilterOperator.GreaterThanOrEqual => "_gte",
                FilterOperator.LessThan => "_lt",
                FilterOperator.LessThanOrEqual => "_lte",
                FilterOperator.Contains => "_ilike",
                FilterOperator.StartsWith => "_ilike",
                FilterOperator.EndsWith => "_ilike",
                FilterOperator.In => "_in",
                FilterOperator.NotIn => "_nin",
                FilterOperator.IsNull => "_is_null",
                FilterOperator.IsNotNull => "_is_null",
                _ => "_eq"
            },
            _ => throw new NotSupportedException($"Convention {convention} not supported")
        };
    }

    /// <summary>
    /// Formats a value based on the operator and convention
    /// </summary>
    public static object? FormatValue(FilterOperator op, object? value, GraphQLConvention convention)
    {
        if (op is FilterOperator.IsNull or FilterOperator.IsNotNull)
        {
            return op == FilterOperator.IsNotNull;
        }

        if (convention == GraphQLConvention.Hasura && value is string str)
        {
            return op switch
            {
                FilterOperator.Contains => $"%{str}%",
                FilterOperator.StartsWith => $"{str}%",
                FilterOperator.EndsWith => $"%{str}",
                _ => str
            };
        }

        return value;
    }
}