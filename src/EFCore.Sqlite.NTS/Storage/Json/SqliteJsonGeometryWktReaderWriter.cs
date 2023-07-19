// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Json;

/// <summary>
///     Reads and writes JSON using the well-known-text format for <see cref="Geometry" /> values.
/// </summary>
public sealed class SqliteJsonGeometryWktReaderWriter : JsonValueReaderWriter<Geometry>
{
    private static readonly WKTReader WktReader = new();

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static SqliteJsonGeometryWktReaderWriter Instance { get; } = new();

    private SqliteJsonGeometryWktReaderWriter()
    {
    }

    /// <inheritdoc />
    public override Geometry FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => WktReader.Read(manager.CurrentReader.GetString());

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, Geometry value)
        => writer.WriteStringValue(value.ToText());
}
