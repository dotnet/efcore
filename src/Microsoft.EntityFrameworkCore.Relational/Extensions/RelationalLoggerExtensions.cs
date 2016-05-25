// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage
{
    internal static class RelationalLoggerExtensions
    {
        public static void LogCommandExecuted(
            [NotNull] this ISensitiveDataLogger logger,
            [NotNull] DbCommand command,
            long startTimestamp,
            long currentTimestamp)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(command, nameof(command));

            if (logger.IsEnabled(LogLevel.Information))
            {
                var logParameterValues
                    = command.Parameters.Count > 0
                      && logger.LogSensitiveData;

                var logData = new DbCommandLogData(
                    command.CommandText.TrimEnd(),
                    command.CommandType,
                    command.CommandTimeout,
                    command.Parameters
                        .Cast<DbParameter>()
                        .Select(
                            p => new DbParameterLogData(
                                p.ParameterName,
                                logParameterValues ? p.Value : "?",
                                logParameterValues,
                                p.Direction,
                                p.DbType,
                                p.IsNullable,
                                p.Size,
                                p.Precision,
                                p.Scale))
                        .ToList(),
                    DeriveTimespan(startTimestamp, currentTimestamp));

                logger.Log(
                    LogLevel.Information,
                    (int)RelationalEventId.ExecutedCommand,
                    logData,
                    null,
                    (state, _) =>
                        {
                            var elapsedMilliseconds = DeriveTimespan(startTimestamp, currentTimestamp);

                            return RelationalStrings.RelationalLoggerExecutedCommand(
                                string.Format($"{elapsedMilliseconds:N0}"),
                                state.Parameters
                                    .Select(p => $"{p.Name}={FormatParameter(p)}")
                                    .Join(),
                                state.CommandType,
                                state.CommandTimeout,
                                Environment.NewLine,
                                state.CommandText);
                        });
            }
        }

        public static string FormatParameter(DbParameterLogData parameterData)
        {
            var builder = new StringBuilder();

            var value = parameterData.Value;
            var clrType = value?.GetType();

            FormatParameterValue(builder, value);

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
                    .Append(parameterData.Size)
                    .Append(')');
            }

            if (parameterData.Precision != 0)
            {
                builder
                    .Append(" (Precision = ")
                    .Append(parameterData.Precision)
                    .Append(')');
            }

            if (parameterData.Scale != 0)
            {
                builder
                    .Append(" (Scale = ")
                    .Append(parameterData.Scale)
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

        public static void FormatParameterValue([NotNull] StringBuilder builder, [CanBeNull] object parameterValue)
        {
            builder.Append('\'');

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

            builder.Append('\'');
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

        public static void LogInformation<TState>(
            this ILogger logger, RelationalEventId eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.Log(LogLevel.Information, (int)eventId, state, null, (s, _) => formatter(s));
            }
        }

        public static void LogDebug(
            this ILogger logger, RelationalEventId eventId, Func<string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log<object>(LogLevel.Debug, (int)eventId, null, null, (_, __) => formatter());
            }
        }

        public static void LogDebug<TState>(
            this ILogger logger, RelationalEventId eventId, TState state, Func<TState, string> formatter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(LogLevel.Debug, (int)eventId, state, null, (s, __) => formatter(s));
            }
        }

        public static void LogWarning(this ILogger logger, RelationalEventId eventId, Func<string> formatter)
        {
            // Always call Log for Warnings because Warnings as Errors should work even
            // if LogLevel.Warning is not enabled.
            logger.Log<object>(LogLevel.Warning, (int)eventId, eventId, null, (_, __) => formatter());
        }

        private static long DeriveTimespan(long startTimestamp, long currentTimestamp)
            => (currentTimestamp - startTimestamp) / TimeSpan.TicksPerMillisecond;
    }
}
