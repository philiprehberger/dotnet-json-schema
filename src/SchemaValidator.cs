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
    /// <param name="rootSchema">The root schema document for resolving $ref references.</param>
    internal static void Validate(JsonNode schema, JsonNode? instance, string path, List<ValidationError> errors, JsonNode? rootSchema = null)
    {
        if (schema is not JsonObject schemaObj)
            return;

        rootSchema ??= schema;

        // $ref — resolve and validate against the referenced schema
        if (schemaObj.TryGetPropertyValue("$ref", out var refNode))
        {
            var refPath = refNode!.GetValue<string>();
            var resolved = ResolveRef(rootSchema, refPath);
            if (resolved is not null)
            {
                Validate(resolved, instance, path, errors, rootSchema);
            }
            else
            {
                errors.Add(new ValidationError(path, $"Could not resolve $ref '{refPath}'.", "$ref"));
            }
            return;
        }

        // allOf — all sub-schemas must match
        if (schemaObj.TryGetPropertyValue("allOf", out var allOfNode) && allOfNode is JsonArray allOfArray)
        {
            foreach (var subSchema in allOfArray)
            {
                if (subSchema is not null)
                {
                    Validate(subSchema, instance, path, errors, rootSchema);
                }
            }
        }

        // anyOf — at least one sub-schema must match
        if (schemaObj.TryGetPropertyValue("anyOf", out var anyOfNode) && anyOfNode is JsonArray anyOfArray)
        {
            bool anyMatch = false;
            foreach (var subSchema in anyOfArray)
            {
                if (subSchema is not null)
                {
                    var subErrors = new List<ValidationError>();
                    Validate(subSchema, instance, path, subErrors, rootSchema);
                    if (subErrors.Count == 0)
                    {
                        anyMatch = true;
                        break;
                    }
                }
            }
            if (!anyMatch)
            {
                errors.Add(new ValidationError(path, "Value does not match any of the schemas in 'anyOf'.", "anyOf"));
            }
        }

        // oneOf — exactly one sub-schema must match
        if (schemaObj.TryGetPropertyValue("oneOf", out var oneOfNode) && oneOfNode is JsonArray oneOfArray)
        {
            int matchCount = 0;
            foreach (var subSchema in oneOfArray)
            {
                if (subSchema is not null)
                {
                    var subErrors = new List<ValidationError>();
                    Validate(subSchema, instance, path, subErrors, rootSchema);
                    if (subErrors.Count == 0)
                    {
                        matchCount++;
                    }
                }
            }
            if (matchCount != 1)
            {
                errors.Add(new ValidationError(path, $"Value must match exactly one schema in 'oneOf', but matched {matchCount}.", "oneOf"));
            }
        }

        // not — must not match the sub-schema
        if (schemaObj.TryGetPropertyValue("not", out var notNode) && notNode is not null)
        {
            var subErrors = new List<ValidationError>();
            Validate(notNode, instance, path, subErrors, rootSchema);
            if (subErrors.Count == 0)
            {
                errors.Add(new ValidationError(path, "Value must not match the schema in 'not'.", "not"));
            }
        }

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

            if (schemaObj.TryGetPropertyValue("format", out var formatNode))
            {
                var format = formatNode!.GetValue<string>();
                if (!ValidateFormat(str, format))
                    errors.Add(new ValidationError(path, $"String does not match format '{format}'.", "format"));
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
                        Validate(prop.Value, propValue, $"{path}.{prop.Key}", errors, rootSchema);
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

            if (schemaObj.TryGetPropertyValue("uniqueItems", out var uniqueNode))
            {
                bool uniqueItems = uniqueNode!.GetValue<bool>();
                if (uniqueItems && HasDuplicates(instanceArray))
                {
                    errors.Add(new ValidationError(path, "Array contains duplicate items but uniqueItems is true.", "uniqueItems"));
                }
            }

            // items
            if (schemaObj.TryGetPropertyValue("items", out var itemsSchema) && itemsSchema is not null)
            {
                for (int i = 0; i < instanceArray.Count; i++)
                {
                    Validate(itemsSchema, instanceArray[i], $"{path}[{i}]", errors, rootSchema);
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

    private static bool ValidateFormat(string value, string format)
    {
        return format switch
        {
            "email" => ValidateEmail(value),
            "date-time" => ValidateDateTime(value),
            "uri" => ValidateUri(value),
            "ipv4" => ValidateIpv4(value),
            "ipv6" => ValidateIpv6(value),
            _ => true // Unknown formats pass validation per JSON Schema spec
        };
    }

    private static bool ValidateEmail(string value)
    {
        // Basic email validation: local@domain with at least one dot in domain
        return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private static bool ValidateDateTime(string value)
    {
        return DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out _);
    }

    private static bool ValidateUri(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static bool ValidateIpv4(string value)
    {
        if (!System.Net.IPAddress.TryParse(value, out var address))
            return false;
        return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
            && value.Split('.').Length == 4;
    }

    private static bool ValidateIpv6(string value)
    {
        if (!System.Net.IPAddress.TryParse(value, out var address))
            return false;
        return address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
    }

    private static bool HasDuplicates(JsonArray array)
    {
        var seen = new HashSet<string>();
        foreach (var item in array)
        {
            var json = item?.ToJsonString() ?? "null";
            if (!seen.Add(json))
                return true;
        }
        return false;
    }

    private static JsonNode? ResolveRef(JsonNode rootSchema, string refPath)
    {
        // Only support local references starting with #/
        if (!refPath.StartsWith("#/"))
            return null;

        var segments = refPath[2..].Split('/');
        JsonNode? current = rootSchema;

        foreach (var segment in segments)
        {
            if (current is not JsonObject obj)
                return null;

            if (!obj.TryGetPropertyValue(segment, out var next))
                return null;

            current = next;
        }

        return current;
    }
}
