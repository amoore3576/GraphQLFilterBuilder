namespace GraphQLFilterBuilder;

/// <summary>
/// Supported GraphQL filter operators
/// </summary>
public enum FilterOperator
{
    // Comparison
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    
    // String
    Contains,
    StartsWith,
    EndsWith,
    
    // Collection
    In,
    NotIn,
    
    // Null checks
    IsNull,
    IsNotNull
}