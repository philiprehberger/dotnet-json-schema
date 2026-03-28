using Xunit;
using Philiprehberger.JsonSchema;

namespace Philiprehberger.JsonSchema.Tests;

public class EnumConstTests
{
    [Fact]
    public void Enum_AcceptsAllowedValue()
    {
        var schema = JsonSchema.Parse("""{ "enum": ["red", "green", "blue"] }""");
        var result = schema.Validate("\"green\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Enum_RejectsDisallowedValue()
    {
        var schema = JsonSchema.Parse("""{ "enum": ["red", "green", "blue"] }""");
        var result = schema.Validate("\"yellow\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "enum");
    }

    [Fact]
    public void Enum_WorksWithMixedTypes()
    {
        var schema = JsonSchema.Parse("""{ "enum": [1, "two", true, null] }""");
        var result = schema.Validate("\"two\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Const_AcceptsExactValue()
    {
        var schema = JsonSchema.Parse("""{ "const": 42 }""");
        var result = schema.Validate("42");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Const_RejectsDifferentValue()
    {
        var schema = JsonSchema.Parse("""{ "const": 42 }""");
        var result = schema.Validate("99");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "const");
    }
}
