namespace Philiprehberger.JsonSchema;

/// <summary>
/// Represents a single validation error with a JSON path, message, and the schema keyword that caused it.
/// </summary>
/// <param name="Path">The JSON Pointer path to the invalid value (e.g. "$.name" or "$.items[0]").</param>
/// <param name="Message">A human-readable description of the error.</param>
/// <param name="Keyword">The JSON Schema keyword that triggered the error (e.g. "type", "required", "minimum").</param>
public sealed record ValidationError(string Path, string Message, string Keyword);
