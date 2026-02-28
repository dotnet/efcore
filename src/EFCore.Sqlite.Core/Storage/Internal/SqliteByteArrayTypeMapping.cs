// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Json.Internal;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteByteArrayTypeMapping : ByteArrayTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqliteByteArrayTypeMapping Default { get; } = new(SqliteTypeMappingSource.BlobTypeName);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteByteArrayTypeMapping(string storeType, DbType? dbType = System.Data.DbType.Binary)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(byte[]),
                    jsonValueReaderWriter: SqliteJsonByteArrayReaderWriter.Instance),
                storeType,
                dbType: dbType),
            false)
    {
    }

    private SqliteByteArrayTypeMapping(
        RelationalTypeMappingParameters parameters,
        bool isJsonColumn)
        : base(parameters)
    {
        _isJsonColumn = isJsonColumn;
    }

    private readonly bool _isJsonColumn;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>


    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteByteArrayTypeMapping(parameters, _isJsonColumn);

    internal SqliteByteArrayTypeMapping WithJsonColumn()
        => new(Parameters, true);

    /// <summary>
    ///     Configures the parameter, setting the <see cref="Microsoft.Data.Sqlite.SqliteParameter.SqliteType" /> to
    ///     <see cref="Microsoft.Data.Sqlite.SqliteType.Text" /> when the mapping is for a JSON column.
    /// </summary>
    /// <param name="parameter">The parameter to be configured.</param>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (_isJsonColumn && parameter is Data.Sqlite.SqliteParameter sqliteParameter)
        {
            sqliteParameter.SqliteType = Data.Sqlite.SqliteType.Text;
        }
    }
}
