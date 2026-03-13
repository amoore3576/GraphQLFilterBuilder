namespace GraphQLFilterBuilder;

/// <summary>
/// Configuration options for GraphQL filter generation
/// </summary>
public class GraphQLFilterOptions
{
    /// <summary>
    /// The GraphQL schema convention to use
    /// </summary>
    public GraphQLConvention Convention { get; set; } = GraphQLConvention.HotChocolate;

    /// <summary>
    /// Whether to convert property names to camelCase
    /// </summary>
    public bool UseCamelCase { get; set; } = true;

    /// <summary>
    /// Default options (Hot Chocolate with camelCase)
    /// </summary>
    public static GraphQLFilterOptions Default => new();

    /// <summary>
    /// Hot Chocolate convention options
    /// </summary>
    public static GraphQLFilterOptions HotChocolate => new()
    {
        Convention = GraphQLConvention.HotChocolate,
        UseCamelCase = true
    };

    /// <summary>
    /// Hasura convention options
    /// </summary>
    public static GraphQLFilterOptions Hasura => new()
    {
        Convention = GraphQLConvention.Hasura,
        UseCamelCase = false
    };
}

/// <summary>
/// Supported GraphQL schema conventions
/// </summary>
public enum GraphQLConvention
{
    /// <summary>
    /// Hot Chocolate / Strawberry Shake convention
    /// </summary>
    HotChocolate,

    /// <summary>
    /// Hasura convention with underscore prefixes
    /// </summary>
    Hasura
}