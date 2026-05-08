// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     A thin wrapper around <see cref="DbDataReader" /> that remaps column ordinals using an index map.
///     Used by FromSql queries where the user's SQL may return columns in any order.
///     The index map translates compile-time projection indices to runtime reader ordinals.
/// </remarks>
public sealed class IndexRemappingDbDataReader(DbDataReader inner, int[] indexMap) : DbDataReader
{
    private int Remap(int ordinal) => indexMap[ordinal];

    /// <inheritdoc />
    public override object this[int ordinal] => inner[Remap(ordinal)];

    /// <inheritdoc />
    public override object this[string name] => inner[name];

    /// <inheritdoc />
    public override int FieldCount => inner.FieldCount;

    /// <inheritdoc />
    public override int Depth => inner.Depth;

    /// <inheritdoc />
    public override bool HasRows => inner.HasRows;

    /// <inheritdoc />
    public override bool IsClosed => inner.IsClosed;

    /// <inheritdoc />
    public override int RecordsAffected => inner.RecordsAffected;

    /// <inheritdoc />
    public override bool GetBoolean(int ordinal) => inner.GetBoolean(Remap(ordinal));

    /// <inheritdoc />
    public override byte GetByte(int ordinal) => inner.GetByte(Remap(ordinal));

    /// <inheritdoc />
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
        => inner.GetBytes(Remap(ordinal), dataOffset, buffer, bufferOffset, length);

    /// <inheritdoc />
    public override char GetChar(int ordinal) => inner.GetChar(Remap(ordinal));

    /// <inheritdoc />
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
        => inner.GetChars(Remap(ordinal), dataOffset, buffer, bufferOffset, length);

    /// <inheritdoc />
    public override string GetDataTypeName(int ordinal) => inner.GetDataTypeName(Remap(ordinal));

    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal) => inner.GetDateTime(Remap(ordinal));

    /// <inheritdoc />
    public override decimal GetDecimal(int ordinal) => inner.GetDecimal(Remap(ordinal));

    /// <inheritdoc />
    public override double GetDouble(int ordinal) => inner.GetDouble(Remap(ordinal));

    /// <inheritdoc />
    public override IEnumerator GetEnumerator() => inner.GetEnumerator();

    /// <inheritdoc />
    public override Type GetFieldType(int ordinal) => inner.GetFieldType(Remap(ordinal));

    /// <inheritdoc />
    public override float GetFloat(int ordinal) => inner.GetFloat(Remap(ordinal));

    /// <inheritdoc />
    public override Guid GetGuid(int ordinal) => inner.GetGuid(Remap(ordinal));

    /// <inheritdoc />
    public override short GetInt16(int ordinal) => inner.GetInt16(Remap(ordinal));

    /// <inheritdoc />
    public override int GetInt32(int ordinal) => inner.GetInt32(Remap(ordinal));

    /// <inheritdoc />
    public override long GetInt64(int ordinal) => inner.GetInt64(Remap(ordinal));

    /// <inheritdoc />
    public override string GetName(int ordinal) => inner.GetName(Remap(ordinal));

    /// <inheritdoc />
    public override int GetOrdinal(string name) => inner.GetOrdinal(name);

    /// <inheritdoc />
    public override string GetString(int ordinal) => inner.GetString(Remap(ordinal));

    /// <inheritdoc />
    public override object GetValue(int ordinal) => inner.GetValue(Remap(ordinal));

    /// <inheritdoc />
    public override int GetValues(object[] values) => inner.GetValues(values);

    /// <inheritdoc />
    public override bool IsDBNull(int ordinal) => inner.IsDBNull(Remap(ordinal));

    /// <inheritdoc />
    public override T GetFieldValue<T>(int ordinal) => inner.GetFieldValue<T>(Remap(ordinal));

    /// <inheritdoc />
    public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken = default)
        => inner.GetFieldValueAsync<T>(Remap(ordinal), cancellationToken);

    /// <inheritdoc />
    public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken = default)
        => inner.IsDBNullAsync(Remap(ordinal), cancellationToken);

    // Row navigation delegates to inner — not remapped
    /// <inheritdoc />
    public override bool NextResult() => inner.NextResult();

    /// <inheritdoc />
    public override bool Read() => inner.Read();

    /// <inheritdoc />
    public override DataTable GetSchemaTable() => inner.GetSchemaTable()!;
}
