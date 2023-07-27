// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDecimalTypeMapping : DecimalTypeMapping
{
    private readonly SqlDbType? _sqlDbType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqlServerDecimalTypeMapping Default { get; } = new("decimal(18, 2)", precision: 18, scale: 2);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDecimalTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Decimal,
        int? precision = null,
        int? scale = null,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.PrecisionAndScale)
        : this(
            new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(decimal), jsonValueReaderWriter: JsonDecimalReaderWriter.Instance),
                    storeType,
                    storeTypePostfix,
                    dbType)
                .WithPrecisionAndScale(precision, scale), sqlDbType)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerDecimalTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
        : base(parameters)
    {
        _sqlDbType = sqlDbType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlDbType? SqlType
        => _sqlDbType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerDecimalTypeMapping(parameters, _sqlDbType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (_sqlDbType != null)
        {
            ((SqlParameter)parameter).SqlDbType = _sqlDbType.Value;
        }

        if (Size.HasValue
            && Size.Value != -1)
        {
            parameter.Size = Size.Value;
        }

        if (Precision.HasValue)
        {
            parameter.Precision = (byte)Precision.Value;
        }

        if (Scale.HasValue)
        {
            parameter.Scale = (byte)Scale.Value;
        }
    }
}
