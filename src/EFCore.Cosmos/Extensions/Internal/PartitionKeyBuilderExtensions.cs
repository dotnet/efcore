// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

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

        var converter = property?.GetTypeMapping().Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        if (value == null)
        {
            builder.AddNullValue();
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

                    builder.Add(stringValue);
                    break;

                case bool boolValue:
                    if (expectedType != null && expectedType != typeof(bool))
                    {
                        CheckType(typeof(bool));
                    }

                    builder.Add(boolValue);
                    break;

                case var _ when value.GetType().IsNumeric():
                    if (expectedType != null && !expectedType.IsNumeric())
                    {
                        CheckType(value.GetType());
                    }

                    builder.Add(Convert.ToDouble(value));
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
    }
}
