using Xunit;
using Philiprehberger.JsonSchema;

namespace Philiprehberger.JsonSchema.Tests;

public class RefTests
{
    [Fact]
    public void Ref_ResolvesDefsReference()
    {
        var schema = JsonSchema.Parse("""
        {
            "type": "object",
            "properties": {
                "address": { "$ref": "#/$defs/Address" }
            },
            "$defs": {
                "Address": {
                    "type": "object",
                    "properties": {
                        "street": { "type": "string" },
                        "city": { "type": "string" }
                    },
                    "required": ["street", "city"]
                }
            }
        }
        """);
        var result = schema.Validate("""{ "address": { "street": "123 Main St", "city": "Springfield" } }""");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Ref_FailsWhenReferencedSchemaDoesNotMatch()
    {
        var schema = JsonSchema.Parse("""
        {
            "type": "object",
            "properties": {
                "address": { "$ref": "#/$defs/Address" }
            },
            "$defs": {
                "Address": {
                    "type": "object",
                    "required": ["street", "city"]
                }
            }
        }
        """);
        var result = schema.Validate("""{ "address": { "street": "123 Main St" } }""");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Ref_ResolvesDefinitionsReference()
    {
        var schema = JsonSchema.Parse("""
        {
            "type": "object",
            "properties": {
                "name": { "$ref": "#/definitions/Name" }
            },
            "definitions": {
                "Name": { "type": "string", "minLength": 1 }
            }
        }
        """);
        var result = schema.Validate("""{ "name": "Alice" }""");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Ref_ReportsErrorForUnresolvableRef()
    {
        var schema = JsonSchema.Parse("""
        {
            "type": "object",
            "properties": {
                "value": { "$ref": "#/$defs/Missing" }
            }
        }
        """);
        var result = schema.Validate("""{ "value": 42 }""");
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Keyword == "$ref");
    }

    [Fact]
    public void Ref_WorksWithNestedProperties()
    {
        var schema = JsonSchema.Parse("""
        {
            "type": "object",
            "properties": {
                "home": { "$ref": "#/$defs/Address" },
                "work": { "$ref": "#/$defs/Address" }
            },
            "$defs": {
                "Address": {
                    "type": "object",
                    "properties": {
                        "zip": { "type": "string", "pattern": "^[0-9]{5}$" }
                    },
                    "required": ["zip"]
                }
            }
        }
        """);
        var result = schema.Validate("""{ "home": { "zip": "12345" }, "work": { "zip": "67890" } }""");
        Assert.True(result.IsValid);
    }
}
