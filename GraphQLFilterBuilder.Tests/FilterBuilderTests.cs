namespace GraphQLFilterBuilder.Tests;

[TestFixture]
public class FilterBuilderTests
{
    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }

    [Test]
    public void SimpleFilter_ShouldGenerateCorrectDictionary()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Where(u => u.Status, "Active")
            .Where(u => u.Age, FilterOperator.GreaterThan, 18)
            .Build();

        // Assert
        Assert.That(filter, Has.Count.EqualTo(2));
        Assert.That(filter["status"], Is.Not.Null);
        Assert.That(filter["age"], Is.Not.Null);

        var statusFilter = filter["status"] as Dictionary<string, object?>;
        Assert.That(statusFilter, Is.Not.Null);
        Assert.That(statusFilter!["eq"], Is.EqualTo("Active"));

        var ageFilter = filter["age"] as Dictionary<string, object?>;
        Assert.That(ageFilter, Is.Not.Null);
        Assert.That(ageFilter!["gt"], Is.EqualTo(18));
    }

    [Test]
    public void StringFilters_ShouldGenerateContainsAndEndsWith()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Contains(u => u.Name, "John")
            .Where(u => u.Email, FilterOperator.EndsWith, "@company.com")
            .Build();

        // Assert
        Assert.That(filter, Has.Count.EqualTo(2));

        var nameFilter = filter["name"] as Dictionary<string, object?>;
        Assert.That(nameFilter, Is.Not.Null);
        Assert.That(nameFilter!["contains"], Is.EqualTo("John"));

        var emailFilter = filter["email"] as Dictionary<string, object?>;
        Assert.That(emailFilter, Is.Not.Null);
        Assert.That(emailFilter!["endsWith"], Is.EqualTo("@company.com"));
    }

    [Test]
    public void CollectionFilters_ShouldGenerateInAndNotNull()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .In(u => u.Status, "Active", "Pending", "Verified")
            .IsNotNull(u => u.Email)
            .Build();

        // Assert
        Assert.That(filter, Has.Count.EqualTo(2));

        var statusFilter = filter["status"] as Dictionary<string, object?>;
        Assert.That(statusFilter, Is.Not.Null);
        var statusValues = statusFilter!["in"] as string[];
        Assert.That(statusValues, Is.Not.Null);
        Assert.That(statusValues, Has.Length.EqualTo(3));
        Assert.That(statusValues, Does.Contain("Active"));
        Assert.That(statusValues, Does.Contain("Pending"));
        Assert.That(statusValues, Does.Contain("Verified"));

        var emailFilter = filter["email"] as Dictionary<string, object?>;
        Assert.That(emailFilter, Is.Not.Null);
        Assert.That(emailFilter!["neq"], Is.EqualTo(true));
    }

    [Test]
    public void OrConditions_ShouldGenerateOrArray()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Where(u => u.Age, FilterOperator.GreaterThanOrEqual, 21)
            .Or(
                f => f.Where(u => u.Status, "Premium"),
                f => f.Where(u => u.Status, "VIP")
            )
            .Build();

        // Assert
        Assert.That(filter, Has.Count.EqualTo(2));
        Assert.That(filter.ContainsKey("age"), Is.True);
        Assert.That(filter.ContainsKey("or"), Is.True);

        var orConditions = filter["or"] as Dictionary<string, object?>[];
        Assert.That(orConditions, Is.Not.Null);
        Assert.That(orConditions, Has.Length.EqualTo(2));

        var firstCondition = orConditions![0]["status"] as Dictionary<string, object?>;
        Assert.That(firstCondition!["eq"], Is.EqualTo("Premium"));

        var secondCondition = orConditions[1]["status"] as Dictionary<string, object?>;
        Assert.That(secondCondition!["eq"], Is.EqualTo("VIP"));
    }

    [Test]
    public void ComplexNestedFilter_ShouldGenerateCorrectStructure()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Where(u => u.Age, FilterOperator.GreaterThan, 18)
            .And(
                f => f.Where(u => u.Status, "Active")
                      .Or(
                          inner => inner.Contains(u => u.Name, "Admin"),
                          inner => inner.In(u => u.Status, "Verified", "Premium")
                      )
            )
            .Build();

        // Assert
        Assert.That(filter.ContainsKey("age"), Is.True);
        Assert.That(filter.ContainsKey("and"), Is.True);

        var andConditions = filter["and"] as Dictionary<string, object?>[];
        Assert.That(andConditions, Is.Not.Null);
        Assert.That(andConditions, Has.Length.EqualTo(1));
    }

    [Test]
    public void IsNullFilter_ShouldGenerateNullCheck()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .IsNull(u => u.Email)
            .Build();

        // Assert
        Assert.That(filter, Has.Count.EqualTo(1));

        var emailFilter = filter["email"] as Dictionary<string, object?>;
        Assert.That(emailFilter, Is.Not.Null);
        Assert.That(emailFilter!["eq"], Is.EqualTo(true));
    }

    [Test]
    public void AllComparisonOperators_ShouldGenerateCorrectKeys()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Where(u => u.Age, FilterOperator.Equal, 18)
            .Where(u => u.Age, FilterOperator.NotEqual, 19)
            .Where(u => u.Age, FilterOperator.GreaterThan, 20)
            .Where(u => u.Age, FilterOperator.GreaterThanOrEqual, 21)
            .Where(u => u.Age, FilterOperator.LessThan, 22)
            .Where(u => u.Age, FilterOperator.LessThanOrEqual, 23)
            .Build();

        // Assert - only the last condition for 'age' will be kept (dictionary key overwrite)
        var ageFilter = filter["age"] as Dictionary<string, object?>;
        Assert.That(ageFilter, Is.Not.Null);
        Assert.That(ageFilter!.ContainsKey("lte"), Is.True);
    }

    [Test]
    public void HasuraConvention_ShouldUseUnderscorePrefixes()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>(GraphQLFilterOptions.Hasura)
            .Where(u => u.Age, FilterOperator.GreaterThan, 18)
            .Contains(u => u.Name, "John")
            .Build();

        // Assert
        Assert.That(filter, Has.Count.EqualTo(2));

        var ageFilter = filter["Age"] as Dictionary<string, object?>;
        Assert.That(ageFilter, Is.Not.Null);
        Assert.That(ageFilter!.ContainsKey("_gt"), Is.True);
        Assert.That(ageFilter["_gt"], Is.EqualTo(18));

        var nameFilter = filter["Name"] as Dictionary<string, object?>;
        Assert.That(nameFilter, Is.Not.Null);
        Assert.That(nameFilter!.ContainsKey("_ilike"), Is.True);
        Assert.That(nameFilter["_ilike"], Is.EqualTo("%John%")); // Hasura wraps with wildcards
    }

    [Test]
    public void CamelCaseOption_ShouldConvertPropertyNames()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>() // Default uses camelCase
            .Where(u => u.Age, FilterOperator.GreaterThan, 18)
            .Where(u => u.Status, "Active")
            .Build();

        // Assert
        Assert.That(filter.ContainsKey("age"), Is.True);
        Assert.That(filter.ContainsKey("status"), Is.True);
        Assert.That(filter.ContainsKey("Age"), Is.False);
        Assert.That(filter.ContainsKey("Status"), Is.False);
    }

    [Test]
    public void NoCamelCase_ShouldKeepOriginalPropertyNames()
    {
        // Arrange
        var options = new GraphQLFilterOptions { UseCamelCase = false };

        // Act
        var filter = new FilterBuilder<User>(options)
            .Where(u => u.Age, FilterOperator.GreaterThan, 18)
            .Build();

        // Assert
        Assert.That(filter.ContainsKey("Age"), Is.True);
        Assert.That(filter.ContainsKey("age"), Is.False);
    }

    [Test]
    public void InWithIEnumerable_ShouldAcceptList()
    {
        // Arrange
        var statuses = new List<string> { "Active", "Pending", "Verified" };

        // Act
        var filter = new FilterBuilder<User>()
            .In(u => u.Status, statuses)
            .Build();

        // Assert
        var statusFilter = filter["status"] as Dictionary<string, object?>;
        Assert.That(statusFilter, Is.Not.Null);
        var statusValues = statusFilter!["in"] as string[];
        Assert.That(statusValues, Has.Length.EqualTo(3));
    }

    [Test]
    public void EmptyFilter_ShouldReturnEmptyDictionary()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Build();

        // Assert
        Assert.That(filter, Is.Empty);
    }

    [Test]
    public void StartsWithOperator_ShouldGenerateCorrectFilter()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Where(u => u.Name, FilterOperator.StartsWith, "John")
            .Build();

        // Assert
        var nameFilter = filter["name"] as Dictionary<string, object?>;
        Assert.That(nameFilter, Is.Not.Null);
        Assert.That(nameFilter!["startsWith"], Is.EqualTo("John"));
    }

    [Test]
    public void NotInOperator_ShouldGenerateCorrectFilter()
    {
        // Arrange & Act
        var filter = new FilterBuilder<User>()
            .Where(u => u.Status, FilterOperator.NotIn, new[] { "Deleted", "Suspended" })
            .Build();

        // Assert
        var statusFilter = filter["status"] as Dictionary<string, object?>;
        Assert.That(statusFilter, Is.Not.Null);
        Assert.That(statusFilter!.ContainsKey("nin"), Is.True);
    }
}