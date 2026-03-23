using System.Text.Json.Nodes;

namespace Philiprehberger.JsonSchema;

/// <summary>
/// Entry point for parsing JSON Schema definitions and validating JSON documents.
/// </summary>
public static class JsonSchema
{
    /// <summary>
    /// Parses a JSON Schema string into a compiled <see cref="Schema"/>.
    /// </summary>
    /// <param name="schemaJson">The JSON Schema definition as a string.</param>
    /// <returns>A compiled <see cref="Schema"/> ready for validation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schemaJson"/> is null.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when <paramref name="schemaJson"/> is not valid JSON.</exception>
    public static Schema Parse(string schemaJson)
    {
        ArgumentNullException.ThrowIfNull(schemaJson);

        var node = JsonNode.Parse(schemaJson)
            ?? throw new ArgumentException("Schema must be a JSON object, not null.", nameof(schemaJson));

        if (node is not JsonObject)
            throw new ArgumentException("Schema must be a JSON object.", nameof(schemaJson));

        return new Schema(node);
    }

    /// <summary>
    /// Validates a JSON document string against a JSON Schema string in one call.
    /// </summary>
    /// <param name="schemaJson">The JSON Schema definition.</param>
    /// <param name="documentJson">The JSON document to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either argument is null.</exception>
    public static ValidationResult Validate(string schemaJson, string documentJson)
    {
        ArgumentNullException.ThrowIfNull(schemaJson);
        ArgumentNullException.ThrowIfNull(documentJson);

        var schema = Parse(schemaJson);
        return schema.Validate(documentJson);
    }
}
