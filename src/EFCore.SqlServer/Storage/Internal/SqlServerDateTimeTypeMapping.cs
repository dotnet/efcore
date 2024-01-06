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
public class SqlServerDateTimeTypeMapping : DateTimeTypeMapping
{
    private const string DateFormatConst = "'{0:yyyy-MM-dd}'";
    private const string SmallDateTimeFormatConst = "'{0:yyyy-MM-ddTHH:mm:ss}'";
    private const string DateTimeFormatConst = "'{0:yyyy-MM-ddTHH:mm:ss.fff}'";

    private readonly SqlDbType? _sqlDbType;

    // Note: this array will be accessed using the precision as an index
    // so the order of the entries in this array is important
    private readonly string[] _dateTime2Formats =
    [
        "'{0:yyyy-MM-ddTHH:mm:ssK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.ffK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fffK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.ffffK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fffffK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.ffffffK}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fffffffK}'"
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqlServerDateTimeTypeMapping Default { get; } = new("datetime2");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDateTimeTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.DateTime2,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(DateTime), jsonValueReaderWriter: JsonDateTimeReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType),
            sqlDbType)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerDateTimeTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
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
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (_sqlDbType != null)
        {
            ((SqlParameter)parameter).SqlDbType = _sqlDbType.Value;
        }
        else if (DbType == System.Data.DbType.Date)
        {
            // Workaround for SqlClient issue: https://github.com/dotnet/runtime/issues/22386
            ((SqlParameter)parameter).SqlDbType = SqlDbType.Date;
        }

        if (Size.HasValue
            && Size.Value != -1)
        {
            parameter.Size = Size.Value;
        }

        if (Precision.HasValue)
        {
            // SQL Server accepts a scale, but in EF a scale along isn't supported (without precision).
            // So the actual value is contained as precision in scale, but sent as Scale to SQL Server.
            parameter.Scale = (byte)Precision.Value;
        }
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerDateTimeTypeMapping(parameters, _sqlDbType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string SqlLiteralFormatString
        => StoreType switch
        {
            "date" => DateFormatConst,
            "datetime" => DateTimeFormatConst,
            "smalldatetime" => SmallDateTimeFormatConst,
            _ => _dateTime2Formats[Precision is >= 0 and <= 7 ? Precision.Value : 7]
        };
}
