# Philiprehberger.JsonSchema

[![CI](https://github.com/philiprehberger/dotnet-json-schema/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-json-schema/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.JsonSchema.svg)](https://www.nuget.org/packages/Philiprehberger.JsonSchema)
[![GitHub release](https://img.shields.io/github/v/release/philiprehberger/dotnet-json-schema)](https://github.com/philiprehberger/dotnet-json-schema/releases)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-json-schema)](https://github.com/philiprehberger/dotnet-json-schema/commits/main)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-json-schema)](LICENSE)
[![Bug Reports](https://img.shields.io/github/issues/philiprehberger/dotnet-json-schema/bug)](https://github.com/philiprehberger/dotnet-json-schema/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![Feature Requests](https://img.shields.io/github/issues/philiprehberger/dotnet-json-schema/enhancement)](https://github.com/philiprehberger/dotnet-json-schema/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

Validate JSON documents against JSON Schema with structured error paths using System.Text.Json.

## Installation

```bash
dotnet add package Philiprehberger.JsonSchema
```

## Usage

```csharp
using Philiprehberger.JsonSchema;

var schema = JsonSchema.Parse("""
{
    "type": "object",
    "properties": {
        "name": { "type": "string", "minLength": 1 },
        "age": { "type": "integer", "minimum": 0 }
    },
    "required": ["name"]
}
""");

var result = schema.Validate("""{ "name": "Alice", "age": 30 }""");
Console.WriteLine(result.IsValid); // True
```

### Handling Validation Errors

```csharp
var result = schema.Validate("""{ "age": -5 }""");

foreach (var error in result.Errors)
{
    Console.WriteLine($"{error.Path}: {error.Message} ({error.Keyword})");
    // $.name: Required property is missing (required)
    // $.age: Value is less than minimum of 0 (minimum)
}
```

### String Validation

```csharp
using Philiprehberger.JsonSchema;

var schema = JsonSchema.Parse("""
{
    "type": "string",
    "minLength": 1,
    "maxLength": 100,
    "pattern": "^[A-Za-z]+$",
    "format": "email"
}
""");
```

Supported formats: `email`, `date-time`, `uri`, `ipv4`, `ipv6`.

### Array Validation

```csharp
using Philiprehberger.JsonSchema;

var schema = JsonSchema.Parse("""
{
    "type": "array",
    "items": { "type": "string" },
    "minItems": 1,
    "maxItems": 10,
    "uniqueItems": true
}
""");
```

### Enum and Const

```csharp
using Philiprehberger.JsonSchema;

var schema = JsonSchema.Parse("""
{
    "type": "object",
    "properties": {
        "status": { "enum": ["active", "inactive", "pending"] },
        "version": { "const": 2 }
    }
}
""");
```

### Schema Composition

```csharp
using Philiprehberger.JsonSchema;

var schema = JsonSchema.Parse("""
{
    "oneOf": [
        { "type": "string", "maxLength": 5 },
        { "type": "integer", "minimum": 0 }
    ]
}
""");
```

Supports `allOf` (all must match), `anyOf` (at least one), `oneOf` (exactly one), and `not` (must not match).

### $ref with $defs

```csharp
using Philiprehberger.JsonSchema;

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
                "street": { "type": "string" },
                "city": { "type": "string" }
            },
            "required": ["street", "city"]
        }
    }
}
""");
```

### Static Validation

```csharp
using Philiprehberger.JsonSchema;

var result = JsonSchema.Validate(schemaJson, documentJson);
```

## API

| Method | Description |
|--------|-------------|
| `JsonSchema.Parse(string)` | Parse a JSON Schema string into a compiled schema |
| `JsonSchema.Validate(string, string)` | Validate a JSON document against a schema string |
| `Schema.Validate(string)` | Validate a JSON document string against a compiled schema |
| `Schema.Validate(JsonNode?)` | Validate a JsonNode against a compiled schema |
| `ValidationResult.IsValid` | Whether validation passed |
| `ValidationResult.Errors` | List of `ValidationError` with path, message, and keyword |

### Supported Keywords

| Category | Keywords |
|----------|----------|
| Type | `type` |
| String | `minLength`, `maxLength`, `pattern`, `format` |
| Numeric | `minimum`, `maximum` |
| Array | `items`, `minItems`, `maxItems`, `uniqueItems` |
| Object | `properties`, `required` |
| Enum/Const | `enum`, `const` |
| Composition | `allOf`, `anyOf`, `oneOf`, `not` |
| References | `$ref`, `$defs`, `definitions` |

## Development

```bash
dotnet build src/Philiprehberger.JsonSchema.csproj --configuration Release
```

## Support

If you find this package useful, consider giving it a star on GitHub — it helps motivate continued maintenance and development.

[![LinkedIn](https://img.shields.io/badge/Philip%20Rehberger-LinkedIn-0A66C2?logo=linkedin)](https://www.linkedin.com/in/philiprehberger)
[![More packages](https://img.shields.io/badge/more-open%20source%20packages-blue)](https://philiprehberger.com/open-source-packages)

## License

[MIT](LICENSE)
