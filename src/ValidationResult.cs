namespace Philiprehberger.JsonSchema;

/// <summary>
/// Represents the outcome of validating a JSON document against a schema.
/// </summary>
/// <param name="IsValid">True if the document is valid against the schema.</param>
/// <param name="Errors">The list of validation errors, empty when valid.</param>
public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    /// <summary>
    /// A successful validation result with no errors.
    /// </summary>
    public static ValidationResult Valid { get; } = new(true, Array.Empty<ValidationError>());

    /// <summary>
    /// Creates a failed validation result from a list of errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A <see cref="ValidationResult"/> representing failure.</returns>
    public static ValidationResult Invalid(IReadOnlyList<ValidationError> errors) => new(false, errors);
}
