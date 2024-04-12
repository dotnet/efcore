﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="HierarchyId" /> values.
/// </summary>
public sealed class SqlServerJsonHierarchyIdReaderWriter : JsonValueReaderWriter<HierarchyId>
{
    private static readonly PropertyInfo InstanceProperty = typeof(SqlServerJsonHierarchyIdReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static SqlServerJsonHierarchyIdReaderWriter Instance { get; } = new();

    private SqlServerJsonHierarchyIdReaderWriter()
    {
    }

    /// <inheritdoc />
    public override HierarchyId FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => new(manager.CurrentReader.GetString()!);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, HierarchyId value)
        => writer.WriteStringValue(value.ToString());

    /// <inheritdoc />
    public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
}
