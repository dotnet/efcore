// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosPropertyExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsPersisted(this IReadOnlyProperty property)
        => property.GetJsonPropertyName() != string.Empty;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string SerializeJsonId(this IProperty property, object? providerValue)
    {
        var mapping = property.GetTypeMapping();
        var readerWriter = mapping.JsonValueReaderWriter!;
#pragma warning disable EF1001 // Internal EF Core API usage.
        if (mapping.Converter is not null
            && readerWriter is IJsonConvertedValueReaderWriter converted)
        {
            readerWriter = converted.InnerReaderWriter;
        }
#pragma warning restore EF1001

        using var document = JsonDocument.Parse(readerWriter.ToJsonString(providerValue));
        return document.RootElement.ValueKind == JsonValueKind.String
            && document.RootElement.GetString() is { Length: > 0 } id
                ? id
                : throw new InvalidOperationException(CosmosStrings.InvalidResourceId);
    }
}
