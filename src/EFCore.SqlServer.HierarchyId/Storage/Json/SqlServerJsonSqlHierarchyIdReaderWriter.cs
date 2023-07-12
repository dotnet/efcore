// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.SqlServer.Types;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="SqlHierarchyId" /> values.
/// </summary>
public sealed class SqlServerJsonSqlHierarchyIdReaderWriter : JsonValueReaderWriter<SqlHierarchyId>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static SqlServerJsonSqlHierarchyIdReaderWriter Instance { get; } = new();

    private SqlServerJsonSqlHierarchyIdReaderWriter()
    {
    }

    /// <inheritdoc />
    public override SqlHierarchyId FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => SqlHierarchyId.Parse(manager.CurrentReader.GetString()!);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, SqlHierarchyId value)
        => writer.WriteStringValue(value.ToString());
}
