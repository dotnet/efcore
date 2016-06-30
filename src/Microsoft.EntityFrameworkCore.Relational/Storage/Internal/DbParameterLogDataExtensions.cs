// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DbParameterLogDataExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string FormatParameter(
            [NotNull] this DbParameterLogData parameterData,
            bool quoteValues = true)
        {
            var builder = new StringBuilder();

            var value = parameterData.Value;
            var clrType = value?.GetType();

            FormatParameterValue(builder, value, quoteValues);

            if (parameterData.IsNullable
                && value != null
                && !clrType.IsNullableType())
            {
                builder.Append(" (Nullable = true)");
            }
            else
            {
                if (!parameterData.IsNullable
                    && parameterData.HasValue
                    && (value == null
                        || clrType.IsNullableType()))
                {
                    builder.Append(" (Nullable = false)");
                }
            }

            if (parameterData.Size != 0)
            {
                builder
                    .Append(" (Size = ")
                    .Append(parameterData.Size.ToString(CultureInfo.InvariantCulture))
                    .Append(')');
            }

            if (parameterData.Precision != 0)
            {
                builder
                    .Append(" (Precision = ")
                    .Append(parameterData.Precision.ToString(CultureInfo.InvariantCulture))
                    .Append(')');
            }

            if (parameterData.Scale != 0)
            {
                builder
                    .Append(" (Scale = ")
                    .Append(parameterData.Scale.ToString(CultureInfo.InvariantCulture))
                    .Append(')');
            }

            if (parameterData.Direction != ParameterDirection.Input)
            {
                builder
                    .Append(" (Direction = ")
                    .Append(parameterData.Direction)
                    .Append(')');
            }

            if (parameterData.HasValue
                && !IsNormalDbType(parameterData.DbType, clrType))
            {
                builder
                    .Append(" (DbType = ")
                    .Append(parameterData.DbType)
                    .Append(')');
            }

            return builder.ToString();
        }

        private static void FormatParameterValue(StringBuilder builder, object parameterValue, bool quoteValues)
        {
            if (quoteValues)
            {
                builder.Append('\'');
            }

            if (parameterValue?.GetType() != typeof(byte[]))
            {
                builder.Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture));
            }
            else
            {
                var buffer = (byte[])parameterValue;
                builder.Append("0x");

                for (var i = 0; i < buffer.Length; i++)
                {
                    if (i > 31)
                    {
                        builder.Append("...");
                        break;
                    }
                    builder.Append(buffer[i].ToString("X2", CultureInfo.InvariantCulture));
                }
            }

            if (quoteValues)
            {
                builder.Append('\'');
            }
        }

        private static bool IsNormalDbType(DbType dbType, Type clrType)
        {
            if (clrType == null)
            {
                return false;
            }

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            switch (dbType)
            {
                case DbType.AnsiString: // Zero
                    return clrType != typeof(string);
                case DbType.Binary:
                    return clrType == typeof(byte[]);
                case DbType.Byte:
                    return clrType == typeof(byte);
                case DbType.Boolean:
                    return clrType == typeof(bool);
                case DbType.Decimal:
                    return clrType == typeof(decimal);
                case DbType.Double:
                    return clrType == typeof(double);
                case DbType.Guid:
                    return clrType == typeof(Guid);
                case DbType.Int16:
                    return clrType == typeof(short);
                case DbType.Int32:
                    return clrType == typeof(int);
                case DbType.Int64:
                    return clrType == typeof(long);
                case DbType.Object:
                    return clrType == typeof(object);
                case DbType.SByte:
                    return clrType == typeof(sbyte);
                case DbType.Single:
                    return clrType == typeof(float);
                case DbType.String:
                    return clrType == typeof(string);
                case DbType.Time:
                    return clrType == typeof(TimeSpan);
                case DbType.UInt16:
                    return clrType == typeof(ushort);
                case DbType.UInt32:
                    return clrType == typeof(uint);
                case DbType.UInt64:
                    return clrType == typeof(ulong);
                case DbType.DateTime2:
                    return clrType == typeof(DateTime);
                case DbType.DateTimeOffset:
                    return clrType == typeof(DateTimeOffset);
                //case DbType.VarNumeric:
                //case DbType.AnsiStringFixedLength:
                //case DbType.StringFixedLength:
                //case DbType.Xml:
                //case DbType.Currency:
                //case DbType.Date:
                //case DbType.DateTime:
                default:
                    return false;
            }
        }
    }
}
