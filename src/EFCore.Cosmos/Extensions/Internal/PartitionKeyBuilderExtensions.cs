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

        if (value == null)
        {
            AddValue(null);
        }
        else
        {
            var expectedType = (converter?.ProviderClrType ?? property?.ClrType)?.UnwrapNullableType();
            switch (value)
            {
                case string stringValue:
                    if (expectedType != null && expectedType != typeof(string))
                    {
                        CheckType(typeof(string));
                    }

                    AddValue(stringValue);
                    break;

                case bool boolValue:
                    if (expectedType != null && expectedType != typeof(bool))
                    {
                        CheckType(typeof(bool));
                    }

                    AddValue(boolValue);
                    break;

                case var _ when value.GetType().IsNumeric():
                    if (expectedType != null && !expectedType.IsNumeric())
                    {
                        CheckType(value.GetType());
                    }

                    AddValue(value);
                    break;

                default:
                    throw new InvalidOperationException(CosmosStrings.PartitionKeyBadValue(value.GetType()));
            }

            void CheckType(Type actualType)
            {
                if (expectedType != null && expectedType != actualType)
                {
                    throw new InvalidOperationException(
                        CosmosStrings.PartitionKeyBadValueType(
                            expectedType.ShortDisplayName(),
                            property!.DeclaringType.DisplayName(),
                            property.Name,
                            actualType.DisplayName()));
                }
            }
        }

        return builder;

        void AddValue(object? providerValue)
        {
            var readerWriter = property?.GetJsonValueReaderWriter();
            if (readerWriter == null
                && property?.DeclaringType.Model.FindTypeMappingConfiguration(property.ClrType.UnwrapNullableType())
                    ?.GetJsonValueReaderWriterType() != null)
            {
                readerWriter = mapping?.JsonValueReaderWriter;
            }
#pragma warning disable EF1001 // Internal EF Core API usage.
            if (readerWriter is IJsonConvertedValueReaderWriter converted
                && ReferenceEquals(converted.Converter, converter))
            {
                readerWriter = converted.InnerReaderWriter;
            }
#pragma warning restore EF1001

            if (readerWriter != null
                && (providerValue != null || readerWriter.HandlesNullWrites))
            {
                if (providerValue != null
                    && !readerWriter.ValueType.IsInstanceOfType(providerValue)
                    && providerValue.GetType().IsNumeric()
                    && readerWriter.ValueType.IsNumeric())
                {
                    providerValue = Convert.ChangeType(providerValue, readerWriter.ValueType, CultureInfo.InvariantCulture);
                }

                using var document = JsonDocument.Parse(readerWriter.ToJsonString(providerValue));
                var jsonValue = document.RootElement;
                switch (jsonValue.ValueKind)
                {
                    case JsonValueKind.Null:
                        builder.AddNullValue();
                        break;
                    case JsonValueKind.String:
                        builder.Add(jsonValue.GetString()!);
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        builder.Add(jsonValue.GetBoolean());
                        break;
                    case JsonValueKind.Number:
                        builder.Add(jsonValue.GetDouble());
                        break;
                    default:
                        throw new InvalidOperationException(CosmosStrings.PartitionKeyBadValue(typeof(JsonElement)));
                }

                return;
            }

            switch (providerValue)
            {
                case null:
                    builder.AddNullValue();
                    break;
                case string stringValue:
                    builder.Add(stringValue);
                    break;
                case bool boolValue:
                    builder.Add(boolValue);
                    break;
                default:
                    builder.Add(Convert.ToDouble(providerValue));
                    break;
            }
        }
    }
}
