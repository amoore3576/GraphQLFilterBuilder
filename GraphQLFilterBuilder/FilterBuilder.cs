namespace GraphQLFilterBuilder;

using System.Linq.Expressions;

/// <summary>
/// Fluent API for building GraphQL filter arguments
/// </summary>
/// <typeparam name="T">The entity type being filtered</typeparam>
public class FilterBuilder<T> where T : class
{
    private readonly FilterExpression _expression = new();
    private readonly GraphQLFilterOptions _options;

    public FilterBuilder(GraphQLFilterOptions? options = null)
    {
        _options = options ?? GraphQLFilterOptions.Default;
    }

    /// <summary>
    /// Adds a filter condition
    /// </summary>
    public FilterBuilder<T> Where(Expression<Func<T, object?>> property, FilterOperator op, object? value)
    {
        var propertyName = GetPropertyName(property);
        var filterKey = _options.UseCamelCase ? propertyName.ToCamelCase() : propertyName;
        var operatorKey = OperatorMapping.GetOperatorKey(op, _options.Convention);

        _expression.Filters[filterKey] = new Dictionary<string, object?>
        {
            [operatorKey] = OperatorMapping.FormatValue(op, value, _options.Convention)
        };

        return this;
    }

    /// <summary>
    /// Adds an equality condition (shorthand)
    /// </summary>
    public FilterBuilder<T> Where(Expression<Func<T, object?>> property, object? value)
    {
        return Where(property, FilterOperator.Equal, value);
    }

    /// <summary>
    /// Adds a contains condition for strings
    /// </summary>
    public FilterBuilder<T> Contains(Expression<Func<T, string>> property, string value)
    {
        Expression<Func<T, object?>> converted = Expression.Lambda<Func<T, object?>>(
            Expression.Convert(property.Body, typeof(object)),
            property.Parameters
        );
        return Where(converted, FilterOperator.Contains, value);
    }

    /// <summary>
    /// Adds an 'in' condition for collections
    /// </summary>
    public FilterBuilder<T> In<TValue>(Expression<Func<T, object?>> property, params TValue[] values)
    {
        return Where(property, FilterOperator.In, values);
    }

    /// <summary>
    /// Adds an 'in' condition for collections
    /// </summary>
    public FilterBuilder<T> In<TValue>(Expression<Func<T, object?>> property, IEnumerable<TValue> values)
    {
        return Where(property, FilterOperator.In, values.ToArray());
    }

    /// <summary>
    /// Adds a null check
    /// </summary>
    public FilterBuilder<T> IsNull(Expression<Func<T, object?>> property)
    {
        return Where(property, FilterOperator.IsNull, null);
    }

    /// <summary>
    /// Adds a not null check
    /// </summary>
    public FilterBuilder<T> IsNotNull(Expression<Func<T, object?>> property)
    {
        return Where(property, FilterOperator.IsNotNull, null);
    }

    /// <summary>
    /// Creates an AND group of conditions
    /// </summary>
    public FilterBuilder<T> And(params Action<FilterBuilder<T>>[] conditions)
    {
        foreach (var condition in conditions)
        {
            var nestedBuilder = new FilterBuilder<T>(_options);
            condition(nestedBuilder);
            _expression.AndConditions.Add(nestedBuilder._expression);
        }
        return this;
    }

    /// <summary>
    /// Creates an OR group of conditions
    /// </summary>
    public FilterBuilder<T> Or(params Action<FilterBuilder<T>>[] conditions)
    {
        foreach (var condition in conditions)
        {
            var nestedBuilder = new FilterBuilder<T>(_options);
            condition(nestedBuilder);
            _expression.OrConditions.Add(nestedBuilder._expression);
        }
        return this;
    }

    /// <summary>
    /// Builds the filter as a dictionary to pass as GraphQL query variables
    /// </summary>
    public Dictionary<string, object?> Build()
    {
        return _expression.ToDictionary();
    }

    private static string GetPropertyName(Expression<Func<T, object?>> property)
    {
        return property.Body switch
        {
            MemberExpression member => member.Member.Name,
            UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
            _ => throw new ArgumentException("Invalid property expression", nameof(property))
        };
    }
}