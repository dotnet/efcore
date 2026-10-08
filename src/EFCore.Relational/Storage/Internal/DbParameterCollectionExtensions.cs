// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class DbParameterCollectionExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string FormatParameters(
        this DbParameterCollection parameters,
        bool logParameterValues)
        => parameters
            .Cast<DbParameter>()
            .Select(p => FormatParameter(p, logParameterValues)).Join();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string FormatParameter(this DbParameter parameter, bool logParameterValues)
        => FormatParameter(
            parameter.ParameterName,
            logParameterValues ? parameter.Value : "?",
            logParameterValues,
            parameter.Direction,
            parameter.DbType,
            parameter.IsNullable,
            parameter.Size,
            parameter.Precision,
            parameter.Scale);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static string FormatParameter(
        string name,
        object? value,
        bool hasValue,
        ParameterDirection direction,
        DbType dbType,
        bool nullable,
        int size,
        byte precision,
        byte scale)
    {
        var builder = new StringBuilder();

        var clrType = value?.GetType();

        builder
            .Append(name)
            .Append('=');

        FormatParameterValue(builder, value);

        if (nullable
            && value != null
            && !clrType!.IsNullableType())
        {
            builder.Append(" (Nullable = true)");
        }
        else
        {
            if (!nullable
                && hasValue
                && (value == null
                    || clrType!.IsNullableType()))
            {
                builder.Append(" (Nullable = false)");
            }
        }

        if (size != 0)
        {
            builder
                .Append(" (Size = ")
                .Append(size.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        if (precision != 0)
        {
            builder
                .Append(" (Precision = ")
                .Append(precision.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        if (scale != 0)
        {
            builder
                .Append(" (Scale = ")
                .Append(scale.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        if (direction != ParameterDirection.Input)
        {
            builder
                .Append(" (Direction = ")
                .Append(direction)
                .Append(')');
        }

        if (ShouldShowDbType(hasValue, dbType, clrType))
        {
            builder
                .Append(" (DbType = ")
                .Append(dbType)
                .Append(')');
        }

        return builder.ToString();
    }

    private static void FormatParameterValue(StringBuilder builder, object? parameterValue)
    {
        switch (parameterValue)
        {
            case null:
            case DBNull:
                builder.Append("NULL");
                return;

            case DateTime dateTime:
                builder
                    .Append('\'')
                    .Append(dateTime.ToString("o"))
                    .Append('\'');
                return;

            case DateTimeOffset dateTimeOffset:
                builder
                    .Append('\'')
                    .Append(dateTimeOffset.ToString("o"))
                    .Append('\'');
                return;

            case byte[] byteArray:
                builder.AppendBytes(byteArray);
                return;

            case IList list:
                builder.Append("{ ");

                // Note: multi-dimensional arrays implement non-generic IList. They do not support indexing, but do support enumeration.
                var isFirst = true;
                foreach (var element in list.Cast<object>().Take(5))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }

                    FormatParameterValue(builder, element);
                }

                if (list.Count > 5)
                {
                    builder.Append(", ...");
                }

                builder.Append(" }");
                return;

            default:
                var type = parameterValue.GetType();
                var valueProperty = type.GetRuntimeProperty("Value");
                if (valueProperty != null
                    && valueProperty.PropertyType != type)
                {
                    var isNullProperty = type.GetRuntimeProperty("IsNull");
                    if (isNullProperty != null
                        && isNullProperty.GetValue(parameterValue) is true)
                    {
                        builder.Append("''");
                    }
                    else
                    {
                        FormatParameterValue(builder, valueProperty.GetValue(parameterValue));
                    }
                }
                else
                {
                    builder
                        .Append('\'')
                        .Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture))
                        .Append('\'');
                }

                return;
        }
    }

    private static bool ShouldShowDbType(bool hasValue, DbType dbType, Type? type)
    {
        if (!hasValue
            || type == null
            || type == typeof(DBNull))
        {
            return dbType != DbType.String;
        }

        type = type.UnwrapNullableType().UnwrapEnumType();

        return dbType switch
        {
            DbType.Binary => type != typeof(byte[]),
            DbType.Byte => type != typeof(byte),
            DbType.Boolean => type != typeof(bool),
            DbType.Decimal => type != typeof(decimal),
            DbType.Double => type != typeof(double),
            DbType.Guid => type != typeof(Guid),
            DbType.Int16 => type != typeof(short),
            DbType.Int32 => type != typeof(int),
            DbType.Int64 => type != typeof(long),
            DbType.Object => type != typeof(object),
            DbType.SByte => type != typeof(sbyte),
            DbType.Single => type != typeof(float),
            DbType.String => type != typeof(string),
            DbType.Time => type != typeof(TimeSpan),
            DbType.UInt16 => type != typeof(ushort),
            DbType.UInt32 => type != typeof(uint),
            DbType.UInt64 => type != typeof(ulong),
            DbType.DateTime2 => type != typeof(DateTime),
            DbType.DateTimeOffset => type != typeof(DateTimeOffset),
            //DbType.AnsiString =>
            //DbType.VarNumeric =>
            //DbType.AnsiStringFixedLength =>
            //DbType.StringFixedLength =>
            //DbType.Xml =>
            //DbType.Currency =>
            //DbType.Date =>
            //DbType.DateTime =>
            _ => true,
        };
    }
}
