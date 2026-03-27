# Philiprehberger.JsonSchema

[![CI](https://github.com/philiprehberger/dotnet-json-schema/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-json-schema/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.JsonSchema.svg)](https://www.nuget.org/packages/Philiprehberger.JsonSchema)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-json-schema)](LICENSE)
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

### Static Validation

```csharp
var result = JsonSchema.Validate(schemaJson, documentJson);
```

## API

| Method | Description |
|--------|-------------|
| `JsonSchema.Parse(string)` | Parse a JSON Schema string into a compiled schema |
| `JsonSchema.Validate(string, string)` | Validate a JSON document against a schema string |
| `Schema.Validate(string)` | Validate a JSON document against a compiled schema |
| `Schema.Validate(JsonNode?)` | Validate a JsonNode against a compiled schema |
| `ValidationResult.IsValid` | Whether validation passed |
| `ValidationResult.Errors` | List of validation errors with paths |

## Development

```bash
dotnet build src/Philiprehberger.JsonSchema.csproj --configuration Release
```

## License

MIT
