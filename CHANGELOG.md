# Changelog

## 0.2.0 (2026-03-27)

- Add string validation keywords (minLength, maxLength, pattern, format)
- Add array validation keywords (minItems, maxItems, uniqueItems)
- Add enum and const value constraints
- Add schema composition (allOf, anyOf, oneOf, not)
- Add $ref support with $defs for reusable schema definitions

## 0.1.1 (2026-03-23)

- Fix NuGet badge URL format

## 0.1.0 (2026-03-22)

- Initial release
- Validate JSON documents against JSON Schema using System.Text.Json
- Support type, properties, required, items, enum, const keywords
- Support pattern, minimum, maximum, minLength, maxLength, minItems, maxItems
- Structured error paths for precise error reporting
