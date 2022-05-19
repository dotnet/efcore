// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerTimeSpanTypeMapping : TimeSpanTypeMapping
{
    // Note: this array will be accessed using the precision as an index
    // so the order of the entries in this array is important
    private readonly string[] _timeFormats =
    {
        @"'{0:hh\:mm\:ss}'",
        @"'{0:hh\:mm\:ss\.F}'",
        @"'{0:hh\:mm\:ss\.FF}'",
        @"'{0:hh\:mm\:ss\.FFF}'",
        @"'{0:hh\:mm\:ss\.FFFF}'",
        @"'{0:hh\:mm\:ss\.FFFFF}'",
        @"'{0:hh\:mm\:ss\.FFFFFF}'",
        @"'{0:hh\:mm\:ss\.FFFFFFF}'"
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerTimeSpanTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Time,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TimeSpan)),
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
    protected SqlServerTimeSpanTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerTimeSpanTypeMapping(parameters);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        // Workaround for a SQLClient bug
        if (DbType == System.Data.DbType.Time)
        {
            ((SqlParameter)parameter).SqlDbType = SqlDbType.Time;
        }
        if (Precision.HasValue)
        {
            parameter.Scale = unchecked((byte)Precision.Value);
        }
    }

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
                if (precision <= 7
                    && precision >= 0)
                {
                    return _timeFormats[precision];
                }
            }

            return _timeFormats[7];
        }
    }

    /// <summary>
    ///     Generates the SQL representation of a literal value without conversion.
    /// </summary>
    /// <param name="value">The literal value.</param>
    /// <returns>
    ///     The generated string.
    /// </returns>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        return value is TimeSpan timeSpan && timeSpan.Milliseconds == 0
            ? string.Format(CultureInfo.InvariantCulture, _timeFormats[0], value) //handle trailing decimal separator when no fractional seconds
            : string.Format(CultureInfo.InvariantCulture, SqlLiteralFormatString, value);
    }
}
