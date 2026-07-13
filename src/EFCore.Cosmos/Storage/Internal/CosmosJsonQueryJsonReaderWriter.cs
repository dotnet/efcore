// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosJsonQueryRawJsonReaderWriter : JsonValueReaderWriter<string>
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonQueryRawJsonReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static CosmosJsonQueryRawJsonReaderWriter Instance { get; } = new();

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);

    /// <inheritdoc/>
    public override string FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => throw new UnreachableException("Query json is only serialized.");

    /// <inheritdoc/>
    public override void ToJsonTyped(Utf8JsonWriter writer, string value)
        => writer.WriteRawValue(value, skipInputValidation: true);
}
