// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping
{
    // Note: this array will be accessed using the precision as an index
    // so the order of the entries in this array is important
    private readonly string[] _dateTimeOffsetFormats =
    [
        "'{0:yyyy-MM-ddTHH:mm:sszzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fzzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.ffzzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fffzzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.ffffzzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fffffzzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.ffffffzzz}'",
        "'{0:yyyy-MM-ddTHH:mm:ss.fffffffzzz}'"
    ];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqlServerDateTimeOffsetTypeMapping Default { get; } = new("datetimeoffset");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerDateTimeOffsetTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.DateTimeOffset,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(DateTimeOffset), jsonValueReaderWriter: JsonDateTimeOffsetReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerDateTimeOffsetTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string SqlLiteralFormatString
    {
        get
        {
            if (Precision.HasValue)
            {
                var precision = Precision.Value;
                if (precision is <= 7 and >= 0)
                {
                    return _dateTimeOffsetFormats[precision];
                }
            }

            return _dateTimeOffsetFormats[7];
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (Size.HasValue
            && Size.Value != -1)
        {
            parameter.Size = Size.Value;
        }

        if (Precision.HasValue)
        {
            // Workaround for inconsistent definition of precision/scale between EF and SQLClient for VarTime types
            parameter.Scale = (byte)Precision.Value;
        }
    }
}
