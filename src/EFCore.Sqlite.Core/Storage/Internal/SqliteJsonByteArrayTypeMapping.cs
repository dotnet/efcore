// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteJsonByteArrayTypeMapping : SqliteByteArrayTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqliteJsonByteArrayTypeMapping Default { get; } = new(SqliteByteArrayTypeMapping.Default);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteJsonByteArrayTypeMapping(SqliteByteArrayTypeMapping mapping)
        : base(mapping)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqliteJsonByteArrayTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteJsonByteArrayTypeMapping(parameters);

    internal static SqliteJsonByteArrayTypeMapping Create(SqliteByteArrayTypeMapping mapping)
        => mapping as SqliteJsonByteArrayTypeMapping
            ?? (mapping.HasDefaultParameters
                ? Default
                : new SqliteJsonByteArrayTypeMapping(mapping));

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        if (parameter is Data.Sqlite.SqliteParameter sqliteParameter)
        {
            sqliteParameter.SqliteType = Data.Sqlite.SqliteType.Text;
        }
    }
}
