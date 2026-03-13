namespace GraphQLFilterBuilder.Tests;

[TestFixture]
public class DynamicFilterBuilderTests
{
    [Test]
    public void BuildFromKeyValuePairs_WithOperators_ShouldGenerateCorrectFilter()
    {
        // Arrange
        var builder = new DynamicFilterBuilder();
        var input = new Dictionary<string, object?>
        {
            { "age.gt", 18 },
            { "status.eq", "Active" },
            { "name.contains", "John" }
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
        
        var ageFilter = result["age"] as Dictionary<string, object?>;
        Assert.That(ageFilter!["gt"], Is.EqualTo(18));

        var statusFilter = result["status"] as Dictionary<string, object?>;
        Assert.That(statusFilter!["eq"], Is.EqualTo("Active"));

        var nameFilter = result["name"] as Dictionary<string, object?>;
        Assert.That(nameFilter!["contains"], Is.EqualTo("John"));
    }

    [Test]
    public void BuildFromSimpleKeyValuePairs_ShouldUseEqualityOperator()
    {
        // Arrange
        var builder = new DynamicFilterBuilder();
        var input = new Dictionary<string, object?>
        {
            { "age", 18 },
            { "status", "Active" }
        };

        // Act
        var result = builder.BuildFromSimpleKeyValuePairs(input);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2));
        
        var ageFilter = result["age"] as Dictionary<string, object?>;
        Assert.That(ageFilter!["eq"], Is.EqualTo(18));

        var statusFilter = result["status"] as Dictionary<string, object?>;
        Assert.That(statusFilter!["eq"], Is.EqualTo("Active"));
    }

    [Test]
    public void BuildFromKeyValuePairs_WithCamelCase_ShouldConvertPropertyNames()
    {
        // Arrange
        var options = new GraphQLFilterOptions { UseCamelCase = true };
        var builder = new DynamicFilterBuilder(options);
        var input = new Dictionary<string, object?>
        {
            { "FirstName.contains", "John" },
            { "LastName.eq", "Doe" }
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        Assert.That(result.ContainsKey("firstName"), Is.True);
        Assert.That(result.ContainsKey("lastName"), Is.True);
        Assert.That(result.ContainsKey("FirstName"), Is.False);
    }

    [Test]
    public void BuildFromKeyValuePairs_WithoutCamelCase_ShouldKeepOriginalNames()
    {
        // Arrange
        var options = new GraphQLFilterOptions { UseCamelCase = false };
        var builder = new DynamicFilterBuilder(options);
        var input = new Dictionary<string, object?>
        {
            { "FirstName.contains", "John" }
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        Assert.That(result.ContainsKey("FirstName"), Is.True);
        Assert.That(result.ContainsKey("firstName"), Is.False);
    }

    [Test]
    public void BuildFromKeyValuePairs_WithVariousOperatorAliases_ShouldParse()
    {
        // Arrange
        var builder = new DynamicFilterBuilder();
        var input = new Dictionary<string, object?>
        {
            { "age.>", 18 },
            { "score.>=", 90 },
            { "name.like", "John" },
            { "status.!=", "Deleted" }
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        Assert.That(result, Has.Count.EqualTo(4));
        
        var ageFilter = result["age"] as Dictionary<string, object?>;
        Assert.That(ageFilter!["gt"], Is.EqualTo(18));

        var scoreFilter = result["score"] as Dictionary<string, object?>;
        Assert.That(scoreFilter!["gte"], Is.EqualTo(90));
    }

    [Test]
    public void BuildFromKeyValuePairs_WithInvalidKeys_ShouldSkipThem()
    {
        // Arrange
        var builder = new DynamicFilterBuilder();
        var input = new Dictionary<string, object?>
        {
            { "age.gt", 18 },
            { "invalidkey", "value" }, // No operator
            { "", "empty" }, // Empty key
            { "age.unknown", 25 } // Unknown operator
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1)); // Only valid 'age.gt' should be processed
    }

    [Test]
    public void BuildFromKeyValuePairs_WithNullValues_ShouldSkipThem()
    {
        // Arrange
        var builder = new DynamicFilterBuilder();
        var input = new Dictionary<string, object?>
        {
            { "age.gt", 18 },
            { "status.eq", null }
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        Assert.That(result, Has.Count.EqualTo(1)); // Only 'age.gt' should be included
    }

    [Test]
    public void BuildFromKeyValuePairs_WithIsNullOperator_ShouldGenerateNullCheck()
    {
        // Arrange
        var builder = new DynamicFilterBuilder();
        var input = new Dictionary<string, object?>
        {
            { "email.isnull", "true" } // Value doesn't matter for null checks
        };

        // Act
        var result = builder.BuildFromKeyValuePairs(input);

        // Assert
        var emailFilter = result["email"] as Dictionary<string, object?>;
        Assert.That(emailFilter!["eq"], Is.EqualTo(true));
    }
}