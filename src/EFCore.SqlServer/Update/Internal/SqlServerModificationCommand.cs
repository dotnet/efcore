// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerModificationCommand : ModificationCommand
{
    // No precision or scale, so that the parameter keeps those of the decimal parsed from JSON
    private static readonly SqlServerDecimalTypeMapping JsonNumberTypeMapping = new("decimal");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerModificationCommand(in ModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerModificationCommand(in NonTrackedModificationCommandParameters modificationCommandParameters)
        : base(modificationCommandParameters)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ProcessSinglePropertyJsonUpdate(ref ColumnModificationParameters parameters)
    {
        // TODO: Move more of this logic to the type mapping. Issue #38036
        var property = parameters.Property!;
        var mapping = property.GetRelationalTypeMapping();
        var propertyProviderClrType = (mapping.Converter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();
        var value = parameters.Value;
#pragma warning disable EF1001 // Internal EF Core API usage.
        var json = property.FindJsonFromPropertyReaderWriter(value);
#pragma warning restore EF1001 // Internal EF Core API usage.

        // A null value is sent as a NULL parameter, unless a JSON reader/writer configured on the property writes it. JSON_MODIFY
        // rejects parameters whose type can't be implicitly converted to nvarchar (e.g. date/time types), even when the value itself
        // is null. Since the value is null the parameter type doesn't affect the result, so keep the property's mapping only for types
        // that JSON_MODIFY accepts directly (string, bool, numeric) and otherwise fall back to a string mapping.
        if (value is null
            && json == null)
        {
            parameters = parameters with
            {
                TypeMapping = propertyProviderClrType == typeof(string)
                || propertyProviderClrType == typeof(bool)
                || propertyProviderClrType.IsNumeric()
                    ? mapping
                    : SqlServerStringTypeMapping.UnicodeDefault
            };

            return;
        }

        // JSON-compatible non-string values (bool, numeric) are sent directly as non-string parameters, unless a JSON reader/writer
        // configured on the property writes them.
        if ((propertyProviderClrType == typeof(bool)
                || propertyProviderClrType.IsNumeric())
            && !property.IsPrimitiveCollection
            && json == null)
        {
            parameters = parameters with { Value = value, TypeMapping = mapping };

            return;
        }

        json ??= mapping.JsonValueReaderWriter?.ToJsonString(value);
        if (json != null)
        {
            if (property.IsPrimitiveCollection)
            {
                // This is a JSON array, so send with the original type mapping, which may indicate the column type is JSON.
                parameters = parameters with { Value = json };

                return;
            }

            // JSON_MODIFY inserts a JSON object or array from JSON_QUERY, but has no way to insert other JSON as is. Instead, a string is
            // wrapped in a simple JSON object to avoid double escaping and extracted with JSON_VALUE, and other values are sent as
            // bit, decimal or float parameters, which JSON_MODIFY writes as JSON true/false and numbers. A number is only sent when
            // JSON_MODIFY writes back the same value, which for a float has 16 significant digits.
            var element = JsonElement.Parse(json);
            parameters = element.ValueKind switch
            {
                JsonValueKind.String => parameters with
                {
                    Value = $$"""{"":{{json}}}""",
                    TypeMapping = parameters.TypeMapping is SqlServerStructuralJsonTypeMapping
                        ? parameters.TypeMapping
                        : SqlServerStructuralJsonTypeMapping.Default
                },
                JsonValueKind.True or JsonValueKind.False => parameters with
                {
                    Value = element.GetBoolean(),
                    TypeMapping = SqlServerBoolTypeMapping.Default
                },
                JsonValueKind.Number when element.TryGetDecimal(out var number)
                    && IsSameNumber(element, number.ToString(CultureInfo.InvariantCulture)) => parameters with
                    {
                        Value = number,
                        TypeMapping = JsonNumberTypeMapping
                    },
                JsonValueKind.Number when element.TryGetDouble(out var number)
                    && IsSameNumber(element, number.ToString("E15", CultureInfo.InvariantCulture)) => parameters with
                    {
                        Value = number,
                        TypeMapping = SqlServerDoubleTypeMapping.Default
                    },
                JsonValueKind.Number => throw new InvalidOperationException(
                    SqlServerStrings.JsonNumberNotPreservedInPartialUpdate(property.DeclaringType.DisplayName(), property.Name)),
                JsonValueKind.Null => parameters with { Value = null, TypeMapping = SqlServerStringTypeMapping.UnicodeDefault },
                _ => parameters with { Value = json, Property = null }
            };

            return;
        }

        if (mapping.Converter != null)
        {
            value = mapping.Converter.ConvertToProvider(value);
        }

        parameters = parameters with
        {
            Value = value,
            TypeMapping = parameters.TypeMapping is SqlServerStructuralJsonTypeMapping
                ? SqlServerStringTypeMapping.UnicodeDefault
                : parameters.TypeMapping
        };

        // Whether a JSON number has the same value as JSON_MODIFY writes for the number sent as a parameter
        static bool IsSameNumber(JsonElement element, string writtenNumber)
            => JsonElement.DeepEquals(element, JsonElement.Parse(writtenNumber));
    }
}
