using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Philiprehberger.JsonSchema;

/// <summary>
/// Core validation logic that checks a <see cref="JsonNode"/> against a compiled schema definition.
/// </summary>
internal static class SchemaValidator
{
    /// <summary>
    /// Validates a JSON node against a schema node and collects errors.
    /// </summary>
    /// <param name="schema">The schema definition node.</param>
    /// <param name="instance">The JSON instance to validate.</param>
    /// <param name="path">The current JSON path for error reporting.</param>
    /// <param name="errors">The error collection to append to.</param>
    internal static void Validate(JsonNode schema, JsonNode? instance, string path, List<ValidationError> errors)
    {
        if (schema is not JsonObject schemaObj)
            return;

        // type
        if (schemaObj.TryGetPropertyValue("type", out var typeNode))
        {
            var expectedType = typeNode!.GetValue<string>();
            if (!MatchesType(instance, expectedType))
            {
                errors.Add(new ValidationError(path, $"Expected type '{expectedType}' but got '{GetJsonType(instance)}'.", "type"));
                return; // No point continuing if type is wrong
            }
        }

        // const
        if (schemaObj.TryGetPropertyValue("const", out var constNode))
        {
            if (!JsonNodesEqual(instance, constNode))
            {
                errors.Add(new ValidationError(path, $"Value does not match const.", "const"));
            }
        }

        // enum
        if (schemaObj.TryGetPropertyValue("enum", out var enumNode) && enumNode is JsonArray enumArray)
        {
            bool found = false;
            foreach (var item in enumArray)
            {
                if (JsonNodesEqual(instance, item))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                errors.Add(new ValidationError(path, "Value is not one of the allowed enum values.", "enum"));
            }
        }

        // String validations
        if (instance is JsonValue stringValue && GetJsonType(instance) == "string")
        {
            var str = stringValue.GetValue<string>();

            if (schemaObj.TryGetPropertyValue("minLength", out var minLenNode))
            {
                int minLen = minLenNode!.GetValue<int>();
                if (str.Length < minLen)
                    errors.Add(new ValidationError(path, $"String length {str.Length} is less than minimum {minLen}.", "minLength"));
            }

            if (schemaObj.TryGetPropertyValue("maxLength", out var maxLenNode))
            {
                int maxLen = maxLenNode!.GetValue<int>();
                if (str.Length > maxLen)
                    errors.Add(new ValidationError(path, $"String length {str.Length} exceeds maximum {maxLen}.", "maxLength"));
            }

            if (schemaObj.TryGetPropertyValue("pattern", out var patternNode))
            {
                var pattern = patternNode!.GetValue<string>();
                if (!Regex.IsMatch(str, pattern))
                    errors.Add(new ValidationError(path, $"String does not match pattern '{pattern}'.", "pattern"));
            }
        }

        // Numeric validations
        if (instance is JsonValue numValue && (GetJsonType(instance) == "number" || GetJsonType(instance) == "integer"))
        {
            double num = numValue.GetValue<double>();

            if (schemaObj.TryGetPropertyValue("minimum", out var minNode))
            {
                double min = minNode!.GetValue<double>();
                if (num < min)
                    errors.Add(new ValidationError(path, $"Value {num} is less than minimum of {min}.", "minimum"));
            }

            if (schemaObj.TryGetPropertyValue("maximum", out var maxNode))
            {
                double max = maxNode!.GetValue<double>();
                if (num > max)
                    errors.Add(new ValidationError(path, $"Value {num} exceeds maximum of {max}.", "maximum"));
            }
        }

        // Object validations
        if (instance is JsonObject instanceObj)
        {
            // required
            if (schemaObj.TryGetPropertyValue("required", out var requiredNode) && requiredNode is JsonArray requiredArray)
            {
                foreach (var req in requiredArray)
                {
                    var propName = req!.GetValue<string>();
                    if (!instanceObj.ContainsKey(propName))
                    {
                        errors.Add(new ValidationError($"{path}.{propName}", "Required property is missing.", "required"));
                    }
                }
            }

            // properties
            if (schemaObj.TryGetPropertyValue("properties", out var propsNode) && propsNode is JsonObject propsObj)
            {
                foreach (var prop in propsObj)
                {
                    if (instanceObj.TryGetPropertyValue(prop.Key, out var propValue) && prop.Value is not null)
                    {
                        Validate(prop.Value, propValue, $"{path}.{prop.Key}", errors);
                    }
                }
            }
        }

        // Array validations
        if (instance is JsonArray instanceArray)
        {
            if (schemaObj.TryGetPropertyValue("minItems", out var minItemsNode))
            {
                int minItems = minItemsNode!.GetValue<int>();
                if (instanceArray.Count < minItems)
                    errors.Add(new ValidationError(path, $"Array has {instanceArray.Count} items, minimum is {minItems}.", "minItems"));
            }

            if (schemaObj.TryGetPropertyValue("maxItems", out var maxItemsNode))
            {
                int maxItems = maxItemsNode!.GetValue<int>();
                if (instanceArray.Count > maxItems)
                    errors.Add(new ValidationError(path, $"Array has {instanceArray.Count} items, maximum is {maxItems}.", "maxItems"));
            }

            // items
            if (schemaObj.TryGetPropertyValue("items", out var itemsSchema) && itemsSchema is not null)
            {
                for (int i = 0; i < instanceArray.Count; i++)
                {
                    Validate(itemsSchema, instanceArray[i], $"{path}[{i}]", errors);
                }
            }
        }
    }

    private static bool MatchesType(JsonNode? node, string expectedType)
    {
        return expectedType switch
        {
            "null" => node is null,
            "boolean" => node is JsonValue v && v.TryGetValue<bool>(out _),
            "integer" => node is JsonValue iv && IsInteger(iv),
            "number" => node is JsonValue nv && (nv.TryGetValue<double>(out _) || nv.TryGetValue<long>(out _)),
            "string" => node is JsonValue sv && sv.TryGetValue<string>(out _),
            "array" => node is JsonArray,
            "object" => node is JsonObject,
            _ => false
        };
    }

    private static bool IsInteger(JsonValue value)
    {
        if (value.TryGetValue<long>(out _))
            return true;
        if (value.TryGetValue<int>(out _))
            return true;
        if (value.TryGetValue<double>(out var d))
            return d == Math.Floor(d) && !double.IsInfinity(d);
        return false;
    }

    private static string GetJsonType(JsonNode? node)
    {
        return node switch
        {
            null => "null",
            JsonObject => "object",
            JsonArray => "array",
            JsonValue v when v.TryGetValue<bool>(out _) => "boolean",
            JsonValue v when IsInteger(v) => "integer",
            JsonValue v when v.TryGetValue<double>(out _) => "number",
            JsonValue v when v.TryGetValue<string>(out _) => "string",
            _ => "unknown"
        };
    }

    private static bool JsonNodesEqual(JsonNode? a, JsonNode? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.ToJsonString() == b.ToJsonString();
    }
}
