using Xunit;
using Philiprehberger.JsonSchema;

namespace Philiprehberger.JsonSchema.Tests;

public class CompositionTests
{
    [Fact]
    public void AllOf_RequiresAllSchemasToMatch()
    {
        var schema = JsonSchema.Parse("""
        {
            "allOf": [
                { "type": "object", "properties": { "name": { "type": "string" } }, "required": ["name"] },
                { "type": "object", "properties": { "age": { "type": "integer" } }, "required": ["age"] }
            ]
        }
        """);
        var result = schema.Validate("""{ "name": "Alice", "age": 30 }""");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AllOf_FailsWhenOneSchemaDoesNotMatch()
    {
        var schema = JsonSchema.Parse("""
        {
            "allOf": [
                { "type": "object", "required": ["name"] },
                { "type": "object", "required": ["age"] }
            ]
        }
        """);
        var result = schema.Validate("""{ "name": "Alice" }""");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void AnyOf_AcceptsWhenAtLeastOneMatches()
    {
        var schema = JsonSchema.Parse("""
        {
            "anyOf": [
                { "type": "string" },
                { "type": "integer" }
            ]
        }
        """);
        var result = schema.Validate("\"hello\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AnyOf_FailsWhenNoneMatch()
    {
        var schema = JsonSchema.Parse("""
        {
            "anyOf": [
                { "type": "string" },
                { "type": "integer" }
            ]
        }
        """);
        var result = schema.Validate("true");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "anyOf");
    }

    [Fact]
    public void OneOf_AcceptsExactlyOneMatch()
    {
        var schema = JsonSchema.Parse("""
        {
            "oneOf": [
                { "type": "string", "maxLength": 5 },
                { "type": "string", "minLength": 10 }
            ]
        }
        """);
        var result = schema.Validate("\"hi\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void OneOf_FailsWhenMultipleMatch()
    {
        var schema = JsonSchema.Parse("""
        {
            "oneOf": [
                { "type": "string" },
                { "type": "string", "minLength": 1 }
            ]
        }
        """);
        var result = schema.Validate("\"hello\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "oneOf");
    }

    [Fact]
    public void Not_RejectsMatchingValue()
    {
        var schema = JsonSchema.Parse("""{ "not": { "type": "string" } }""");
        var result = schema.Validate("\"hello\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "not");
    }

    [Fact]
    public void Not_AcceptsNonMatchingValue()
    {
        var schema = JsonSchema.Parse("""{ "not": { "type": "string" } }""");
        var result = schema.Validate("42");
        Assert.True(result.IsValid);
    }
}
