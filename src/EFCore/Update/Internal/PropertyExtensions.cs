// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class PropertyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? ConvertToProviderValue(this IProperty property, object? value)
    {
        var typeMapping = property.GetTypeMapping();
        value = value?.GetType().IsInteger() == true && typeMapping.ClrType.UnwrapNullableType().IsEnum
            ? Enum.ToObject(typeMapping.ClrType.UnwrapNullableType(), value)
            : value;

        var converter = typeMapping.Converter;
        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        return value;
    }
}
