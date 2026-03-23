using System.Text.Json.Nodes;

namespace Philiprehberger.JsonSchema;

/// <summary>
/// A compiled JSON Schema that can validate JSON documents.
/// </summary>
public sealed class Schema
{
    private readonly JsonNode _schemaNode;

    internal Schema(JsonNode schemaNode)
    {
        _schemaNode = schemaNode;
    }

    /// <summary>
    /// Validates a JSON string against this schema.
    /// </summary>
    /// <param name="json">The JSON document string to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="System.Text.Json.JsonException">Thrown when <paramref name="json"/> is not valid JSON.</exception>
    public ValidationResult Validate(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var node = JsonNode.Parse(json);
        return Validate(node);
    }

    /// <summary>
    /// Validates a <see cref="JsonNode"/> against this schema.
    /// </summary>
    /// <param name="node">The JSON node to validate, or null for JSON null.</param>
    /// <returns>A <see cref="ValidationResult"/> describing the outcome.</returns>
    public ValidationResult Validate(JsonNode? node)
    {
        var errors = new List<ValidationError>();
        SchemaValidator.Validate(_schemaNode, node, "$", errors);

        return errors.Count == 0
            ? ValidationResult.Valid
            : ValidationResult.Invalid(errors);
    }
}
