// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
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
            [NotNull] this DbParameterCollection parameters,
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
        public static string FormatParameter([NotNull] this DbParameter parameter, bool logParameterValues)
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
            [NotNull] string name,
            [CanBeNull] object value,
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
                .Append("=");

            FormatParameterValue(builder, value);

            if (nullable
                && value != null
                && !clrType.IsNullableType())
            {
                builder.Append(" (Nullable = true)");
            }
            else
            {
                if (!nullable
                    && hasValue
                    && (value == null
                        || clrType.IsNullableType()))
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

        private static void FormatParameterValue(StringBuilder builder, object parameterValue)
        {
            if (parameterValue == null
                || parameterValue == DBNull.Value)
            {
                builder.Append("NULL");
            }
            else if (parameterValue.GetType() == typeof(DateTime))
            {
                builder
                    .Append('\'')
                    .Append(((DateTime)parameterValue).ToString("o"))
                    .Append('\'');
            }
            else if (parameterValue.GetType() == typeof(DateTimeOffset))
            {
                builder
                    .Append('\'')
                    .Append(((DateTimeOffset)parameterValue).ToString("o"))
                    .Append('\'');
            }
            else if (parameterValue.GetType() == typeof(byte[]))
            {
                builder.AppendBytes((byte[])parameterValue);
            }
            else
            {
                var valueProperty = parameterValue.GetType().GetRuntimeProperty("Value");
                if (valueProperty != null
                    && valueProperty.PropertyType != parameterValue.GetType())
                {
                    var isNullProperty = parameterValue.GetType().GetRuntimeProperty("IsNull");
                    if (isNullProperty != null
                        && (bool)isNullProperty.GetValue(parameterValue))
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
            }
        }

        private static bool ShouldShowDbType(bool hasValue, DbType dbType, Type type)
        {
            if (!hasValue
                || type == null
                || type == typeof(DBNull))
            {
                return dbType != DbType.String;
            }

            type = type.UnwrapNullableType().UnwrapEnumType();

            switch (dbType)
            {
                case DbType.Binary:
                    return type != typeof(byte[]);
                case DbType.Byte:
                    return type != typeof(byte);
                case DbType.Boolean:
                    return type != typeof(bool);
                case DbType.Decimal:
                    return type != typeof(decimal);
                case DbType.Double:
                    return type != typeof(double);
                case DbType.Guid:
                    return type != typeof(Guid);
                case DbType.Int16:
                    return type != typeof(short);
                case DbType.Int32:
                    return type != typeof(int);
                case DbType.Int64:
                    return type != typeof(long);
                case DbType.Object:
                    return type != typeof(object);
                case DbType.SByte:
                    return type != typeof(sbyte);
                case DbType.Single:
                    return type != typeof(float);
                case DbType.String:
                    return type != typeof(string);
                case DbType.Time:
                    return type != typeof(TimeSpan);
                case DbType.UInt16:
                    return type != typeof(ushort);
                case DbType.UInt32:
                    return type != typeof(uint);
                case DbType.UInt64:
                    return type != typeof(ulong);
                case DbType.DateTime2:
                    return type != typeof(DateTime);
                case DbType.DateTimeOffset:
                    return type != typeof(DateTimeOffset);
                //case DbType.AnsiString:
                //case DbType.VarNumeric:
                //case DbType.AnsiStringFixedLength:
                //case DbType.StringFixedLength:
                //case DbType.Xml:
                //case DbType.Currency:
                //case DbType.Date:
                //case DbType.DateTime:
                default:
                    return true;
            }
        }
    }
}
