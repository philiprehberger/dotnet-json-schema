using Xunit;
using Philiprehberger.JsonSchema;

namespace Philiprehberger.JsonSchema.Tests;

public class ArrayValidationTests
{
    [Fact]
    public void MinItems_RejectsTooFewItems()
    {
        var schema = JsonSchema.Parse("""{ "type": "array", "minItems": 2 }""");
        var result = schema.Validate("""[1]""");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "minItems");
    }

    [Fact]
    public void MaxItems_RejectsTooManyItems()
    {
        var schema = JsonSchema.Parse("""{ "type": "array", "maxItems": 2 }""");
        var result = schema.Validate("""[1, 2, 3]""");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "maxItems");
    }

    [Fact]
    public void UniqueItems_RejectsDuplicates()
    {
        var schema = JsonSchema.Parse("""{ "type": "array", "uniqueItems": true }""");
        var result = schema.Validate("""[1, 2, 1]""");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "uniqueItems");
    }

    [Fact]
    public void UniqueItems_AcceptsDistinctValues()
    {
        var schema = JsonSchema.Parse("""{ "type": "array", "uniqueItems": true }""");
        var result = schema.Validate("""[1, 2, 3]""");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MinMaxItems_AcceptsValidArray()
    {
        var schema = JsonSchema.Parse("""{ "type": "array", "minItems": 1, "maxItems": 3 }""");
        var result = schema.Validate("""[1, 2]""");
        Assert.True(result.IsValid);
    }
}
