// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class PartitionKeyBuilderExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static PartitionKeyBuilder Add(this PartitionKeyBuilder builder, object? value, IProperty? property)
    {
        if (value is not null && value.GetType() is var clrType && clrType.IsInteger() && property is not null)
        {
            var unwrappedType = property.ClrType.UnwrapNullableType();
            value = unwrappedType.IsEnum
                ? Enum.ToObject(unwrappedType, value)
                : unwrappedType == typeof(char)
                    ? Convert.ChangeType(value, unwrappedType)
                    : value;
        }

        var mapping = property?.GetTypeMapping();
        var converter = mapping?.Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        if (value != null)
        {
            var valueType = value.GetType();
            if (value is not string and not bool && !valueType.IsNumeric())
            {
                throw new InvalidOperationException(CosmosStrings.PartitionKeyBadValue(valueType));
            }

            var expectedType = (converter?.ProviderClrType ?? property?.ClrType)?.UnwrapNullableType();
            if (expectedType != null
                && expectedType != valueType
                && !(expectedType.IsNumeric() && valueType.IsNumeric()))
            {
                throw new InvalidOperationException(
                    CosmosStrings.PartitionKeyBadValueType(
                        expectedType.ShortDisplayName(),
                        property!.DeclaringType.DisplayName(),
                        property.Name,
                        valueType.DisplayName()));
            }
        }

        var readerWriter = mapping?.JsonValueReaderWriter;
#pragma warning disable EF1001 // Internal EF Core API usage.
        if (readerWriter is IJsonConvertedValueReaderWriter converted
            && ReferenceEquals(converted.Converter, converter))
        {
            readerWriter = converted.InnerReaderWriter;
        }
#pragma warning restore EF1001

        if (readerWriter != null
            && (value != null || readerWriter.HandlesNullWrites))
        {
            if (value != null
                && !readerWriter.ValueType.IsInstanceOfType(value)
                && value.GetType().IsNumeric()
                && readerWriter.ValueType.IsNumeric())
            {
                // Unconfigured numeric keys must not be narrowed to the property's CLR type.
                if (property!.GetJsonValueReaderWriter() == null
                    && property.DeclaringType.Model.FindTypeMappingConfiguration(property.ClrType.UnwrapNullableType())
                        ?.GetJsonValueReaderWriterType() == null)
                {
                    return builder.Add(Convert.ToDouble(value));
                }

                value = Convert.ChangeType(value, readerWriter.ValueType, CultureInfo.InvariantCulture);
            }

            using var document = JsonDocument.Parse(readerWriter.ToJsonString(value));
            value = document.RootElement.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => document.RootElement.GetString(),
                JsonValueKind.True or JsonValueKind.False => document.RootElement.GetBoolean(),
                JsonValueKind.Number => document.RootElement.GetDouble(),
                _ => throw new InvalidOperationException(CosmosStrings.PartitionKeyBadValue(typeof(JsonElement)))
            };
        }

        return value switch
        {
            null => builder.AddNullValue(),
            string stringValue => builder.Add(stringValue),
            bool boolValue => builder.Add(boolValue),
            _ => builder.Add(Convert.ToDouble(value))
        };
    }
}
