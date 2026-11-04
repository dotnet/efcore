// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalJsonPropertyExtensions
{
    /// <summary>
    ///     Returns the JSON written for the value by a JSON reader/writer configured on a property that isn't a primitive collection,
    ///     or <see langword="null" /> if there is none, it is the one configured on enums by convention, which writes the same JSON as
    ///     the property's type mapping, or the value is <see langword="null" /> and the reader/writer doesn't handle null writes.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    public static string? FindJsonFromPropertyReaderWriter(this IProperty property, object? value)
    {
        if (property.IsPrimitiveCollection
            || property.GetJsonValueReaderWriter() is not { } jsonValueReaderWriter
            || (value == null && !jsonValueReaderWriter.HandlesNullWrites)
            || (jsonValueReaderWriter.GetType() is { IsGenericType: true } readerWriterType
                && readerWriterType.GetGenericTypeDefinition() == typeof(JsonWarningEnumReaderWriter<>)))
        {
            return null;
        }

        return jsonValueReaderWriter.ToJsonString(value);
    }
}
