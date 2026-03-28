using Xunit;
using Philiprehberger.JsonSchema;

namespace Philiprehberger.JsonSchema.Tests;

public class StringValidationTests
{
    [Fact]
    public void MinLength_RejectsShortString()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "minLength": 3 }""");
        var result = schema.Validate("\"ab\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "minLength");
    }

    [Fact]
    public void MaxLength_RejectsLongString()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "maxLength": 5 }""");
        var result = schema.Validate("\"toolong\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "maxLength");
    }

    [Fact]
    public void Pattern_RejectsNonMatchingString()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "pattern": "^[a-z]+$" }""");
        var result = schema.Validate("\"ABC123\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "pattern");
    }

    [Fact]
    public void Pattern_AcceptsMatchingString()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "pattern": "^[a-z]+$" }""");
        var result = schema.Validate("\"hello\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Format_Email_RejectsInvalidEmail()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "email" }""");
        var result = schema.Validate("\"not-an-email\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "format");
    }

    [Fact]
    public void Format_Email_AcceptsValidEmail()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "email" }""");
        var result = schema.Validate("\"user@example.com\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Format_DateTime_RejectsInvalidDateTime()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "date-time" }""");
        var result = schema.Validate("\"not-a-date\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "format");
    }

    [Fact]
    public void Format_DateTime_AcceptsValidDateTime()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "date-time" }""");
        var result = schema.Validate("\"2026-03-27T10:00:00Z\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Format_Uri_RejectsInvalidUri()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "uri" }""");
        var result = schema.Validate("\"not a uri\"");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "format");
    }

    [Fact]
    public void Format_Ipv4_AcceptsValidAddress()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "ipv4" }""");
        var result = schema.Validate("\"192.168.1.1\"");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Format_Ipv6_AcceptsValidAddress()
    {
        var schema = JsonSchema.Parse("""{ "type": "string", "format": "ipv6" }""");
        var result = schema.Validate("\"::1\"");
        Assert.True(result.IsValid);
    }
}
