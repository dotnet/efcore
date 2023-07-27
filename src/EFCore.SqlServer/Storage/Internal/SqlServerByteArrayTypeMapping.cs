// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerByteArrayTypeMapping : ByteArrayTypeMapping
{
    private const int MaxSize = 8000;

    private readonly SqlDbType? _sqlDbType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new SqlServerByteArrayTypeMapping Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerByteArrayTypeMapping(
        string? storeType = null,
        int? size = null,
        bool fixedLength = false,
        ValueComparer? comparer = null,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix? storeTypePostfix = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(byte[]), null, comparer, jsonValueReaderWriter: JsonByteArrayReaderWriter.Instance),
                storeType ?? (fixedLength ? "binary" : "varbinary"),
                storeTypePostfix ?? StoreTypePostfix.Size,
                System.Data.DbType.Binary,
                size: size,
                fixedLength: fixedLength),
            sqlDbType)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqlServerByteArrayTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
        : base(parameters)
    {
        _sqlDbType = sqlDbType;
    }

    private static int CalculateSize(int? size)
        => size is > 0 and < MaxSize ? size.Value : MaxSize;

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqlServerByteArrayTypeMapping(parameters, _sqlDbType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void ConfigureParameter(DbParameter parameter)
    {
        var value = parameter.Value;
        var length = (value as byte[])?.Length;
        var maxSpecificSize = CalculateSize(Size);

        if (_sqlDbType.HasValue
            && parameter is SqlParameter sqlParameter) // To avoid crashing wrapping providers
        {
            sqlParameter.SqlDbType = _sqlDbType.Value;
        }

        if (value == null
            || value == DBNull.Value)
        {
            parameter.Size = maxSpecificSize;
        }
        else
        {
            if (length != null
                && length <= maxSpecificSize)
            {
                // Fixed-sized parameters get exact length to avoid padding/truncation.
                parameter.Size = IsFixedLength ? length.Value : maxSpecificSize;
            }
            else if (length is <= MaxSize)
            {
                parameter.Size = IsFixedLength ? length.Value : MaxSize;
            }
            else
            {
                parameter.Size = -1;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var builder = new StringBuilder();
        builder.Append("0x");

        foreach (var @byte in (byte[])value)
        {
            builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}
