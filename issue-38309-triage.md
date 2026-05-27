# AI Triage — dotnet/efcore#38309

The below is an AI-generated analysis and may contain inaccuracies.

**Issue:** [#38309 — Unable to deserialize decimal stored as string from json using ComplexProperty and ToJson (EF Core 10 + Npgsql)](https://github.com/dotnet/efcore/issues/38309)

## Classification

- **Type:** Feature request (not a bug). EF Core's JSON materializer does not currently honor `[System.Text.Json.Serialization.JsonConverter]` attributes on CLR properties mapped to JSON via `ToJson()`, and there is no way to attach a `HasConversion(...)` at the root of a JSON-mapped complex property and have it apply recursively. The reported failure (`Cannot get the value of a token type 'String' as a number`) is the by-design consequence of those missing features when the database already contains legacy JSON in which numbers are encoded as strings.
- **Suggested labels:**
  - `type-enhancement`
  - `area-json`
  - `area-complex-types`
  - (Keep `customer-reported`.)
  - **No** provider-specific label: although the reporter uses Npgsql, the failing code path is in EF Core itself — `RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.MaterializeJsonStructuralType` (visible in the stack trace) is the core relational JSON shaper and the same behavior would reproduce on SQL Server/SQLite when `ToJson()` is used on a complex property containing `decimal` properties whose stored value is a JSON string.

## Summary of the request

The user migrated from Newtonsoft-serialized JSON columns to EF Core 10 `ComplexProperty(...).ToJson()` mapped to PostgreSQL `jsonb`. For backward compatibility with existing data and their public HTTP API, `decimal` values must be serialized in JSON as strings (e.g. `"2.00000000000"`). They report:

1. `[JsonConverter(typeof(...))]` attributes on CLR properties are ignored by EF's JSON mapping.
2. Configuring `JsonSerializerOptions` with `NumberHandling = AllowReadingFromString` via Npgsql's `ConfigureJsonOptions` does not help (EF's JSON shaper does not consume those options for relational JSON columns).
3. The only working workaround is to call `HasConversion(...)` individually on every nested scalar `decimal` property via `ComplexTypePropertyBuilder` / `ComplexCollectionTypePropertyBuilder`, which is verbose, error-prone (easy to miss a newly added nested property), and duplicates rules that already live in the API-layer `JsonConverter`.

They ask for any of:
- Honor `[JsonConverter]` (System.Text.Json) on properties inside JSON-mapped types.
- Allow `HasConversion(...)` on the root `ComplexProperty(...).ToJson(...)` with recursive application to matching scalar types.
- A convention-style API such as `complexProperty.ConfigureProperties(p => p.ClrType == typeof(decimal), p => p.HasConversion(...))`.
- Documentation for the "string-encoded decimals in JSON columns" scenario.

## Repro / verification

A minimal repro is included verbatim in the issue (PostgreSQL + `jsonb` + a `ComplexProperty.ToJson()` containing two `decimal` properties annotated with a custom `[JsonConverter]`). The behavior is **not** a regression — EF has never honored `[JsonConverter]` on JSON-mapped properties (see #27828 below, opened in 2022). The exception thrown is the expected runtime consequence of the JSON shaper invoking `Utf8JsonReader.GetDecimal()` on a `String` token. No reproduction in code is needed beyond what the user already provided.

## Likely duplicates / strongly related issues

| Issue | Title | Relationship |
| --- | --- | --- |
| [#27828](https://github.com/dotnet/efcore/issues/27828) | JSON: Support `[JsonConverter]` | **Primary duplicate / parent feature.** Tracks honoring `System.Text.Json` `[JsonConverter]` on properties inside JSON-mapped types. This issue is essentially another customer ask for that work. |
| [#28043](https://github.com/dotnet/efcore/issues/28043) | Relational: Allow a custom serializer for JSON columns | Related — would also unblock the user (plug in their own serializer for the JSON subtree). |
| [#17194](https://github.com/dotnet/efcore/issues/17194) | `HasConversion` support for nested properties | Related — covers the "apply a conversion recursively to nested scalar properties" angle. |
| [#28933](https://github.com/dotnet/efcore/issues/28933) | Mapping attribute (aka data annotation) for property mapped to JSON column | Related — attribute-driven configuration for JSON-mapped properties. |
| [#31113](https://github.com/dotnet/efcore/issues/31113) | Json serializer options for JSON columns | Related — exposing `JsonSerializerOptions` to the JSON shaper, which would naturally cover `NumberHandling.AllowReadingFromString`. |
| [#30727](https://github.com/dotnet/efcore/issues/30727) | JSON type representations and conversions to store types | Related — broader meta-issue about JSON type representations. |
| [#30330](https://github.com/dotnet/efcore/issues/30330) | Json: updating property with conversion from string to other type fails on sql server | Related historical bug in the same area. |

Recommendation: close as a duplicate of **#27828**, optionally cross-linking #17194 and #31113. The user's "any of these would help" list maps almost one-to-one onto the existing issues above.

## Suggested response to the reporter

Acknowledge that:

- This is by design today and is tracked as a feature request, primarily under #27828 (honor `[JsonConverter]`), with closely related asks in #17194 (recursive `HasConversion`), #28043 (custom serializer for JSON columns), and #31113 (expose `JsonSerializerOptions`).
- Until those land, the documented workaround is exactly what they already discovered: configure `HasConversion(...)` per scalar property inside the complex type. They can reduce duplication by extracting a small helper (e.g. an extension method `ComplexTypePropertyBuilder<decimal>.HasStringDecimalConversion("0.00000000000")`) and applying it from a custom convention or a `ModelConfigurationBuilder` so new nested `decimal` properties are picked up automatically.
- Note that `JsonSerializerOptions` configured on the Npgsql data source (#1107 in `npgsql/efcore.pg`) does not flow into EF's relational JSON materializer — EF uses its own `Utf8JsonReader`-based shaper for `ToJson()` columns; that is what #31113 is asking to change.
