// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using System.Reflection;
#if !NETSTANDARD1_3
using System.Runtime.Remoting.Messaging;

#endif

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class TestSqlLoggerFactory : ILoggerFactory
    {
        private static SqlLogger _logger;
        private static readonly string EOL = Environment.NewLine;

        public ILogger CreateLogger(string name) => Logger;

        private static SqlLogger Logger => LazyInitializer.EnsureInitialized(ref _logger);

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public CancellationToken CancelQuery()
        {
            Logger.SqlLoggerData._cancellationTokenSource = new CancellationTokenSource();

            return Logger.SqlLoggerData._cancellationTokenSource.Token;
        }

        public static void Reset() => Logger.ResetLoggerData();

        public static void CaptureOutput(ITestOutputHelper testOutputHelper)
            => Logger.SqlLoggerData._testOutputHelper = testOutputHelper;

        public void Dispose()
        {
        }

        public static string Log => Logger.SqlLoggerData.LogText;

        public static string Sql
            => string.Join(EOL + EOL, Logger.SqlLoggerData._sqlStatements);

        public static IReadOnlyList<string> SqlStatements => Logger.SqlLoggerData._sqlStatements;

        public static IReadOnlyList<DbCommandLogData> CommandLogData => Logger.SqlLoggerData._logData;

#if NET451
        [Serializable]
#endif
        private class SqlLoggerData
        {
            public string LogText => _log.ToString();

            // ReSharper disable InconsistentNaming
#if NET451
            [NonSerialized]
#endif
            public readonly IndentedStringBuilder _log = new IndentedStringBuilder();
            public readonly List<string> _sqlStatements = new List<string>();
#if NET451
            [NonSerialized]
#endif
            public readonly List<DbCommandLogData> _logData = new List<DbCommandLogData>();
#if NET451
            [NonSerialized]
#endif
            public ITestOutputHelper _testOutputHelper;
#if NET451
            [NonSerialized]
#endif
            public CancellationTokenSource _cancellationTokenSource;
            // ReSharper restore InconsistentNaming
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SqlLogger : ILogger
        {
#if NETSTANDARD1_3
            private readonly static AsyncLocal<SqlLoggerData> _loggerData = new AsyncLocal<SqlLoggerData>();
#else
            private const string ContextName = "__SQL";
#endif

            // ReSharper disable once MemberCanBeMadeStatic.Local
            public SqlLoggerData SqlLoggerData
            {
                get
                {
#if NETSTANDARD1_3
                    var loggerData = _loggerData.Value;
#else
                    var loggerData = (SqlLoggerData)CallContext.LogicalGetData(ContextName);
#endif
                    return loggerData ?? CreateLoggerData();
                }
            }

            private static SqlLoggerData CreateLoggerData()
            {
                var loggerData = new SqlLoggerData();
#if NETSTANDARD1_3
                _loggerData.Value = loggerData;
#else
                CallContext.LogicalSetData(ContextName, loggerData);
#endif
                return loggerData;
            }

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var format = formatter(state, exception)?.Trim();

                if (format != null)
                {
                    var sqlLoggerData = SqlLoggerData;

                    if (sqlLoggerData._cancellationTokenSource != null)
                    {
                        sqlLoggerData._cancellationTokenSource.Cancel();
                        sqlLoggerData._cancellationTokenSource = null;
                    }

                    var commandLogData = state as DbCommandLogData;

                    if (commandLogData != null)
                    {
                        var parameters = "";

                        if (commandLogData.Parameters.Any())
                        {
                            parameters
                                = string.Join(
                                    EOL,
                                    commandLogData.Parameters
                                        .Select(p => $"{p.Name}: {FormatParameter(p)}"))
                                    + EOL + EOL;
                        }

                        sqlLoggerData._sqlStatements.Add(parameters + commandLogData.CommandText);

                        sqlLoggerData._logData.Add(commandLogData);
                    }

                    else
                    {
                        sqlLoggerData._log.AppendLine(format);
                    }

                    sqlLoggerData._testOutputHelper?.WriteLine(format + Environment.NewLine);
                }
            }


            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => SqlLoggerData._log.Indent();

            // ReSharper disable once MemberCanBeMadeStatic.Local
            public void ResetLoggerData() =>
#if NETSTANDARD1_3
                    _loggerData.Value = null;
#else
                CallContext.LogicalSetData(ContextName, null);
#endif
        }

        public static string FormatParameter(DbParameterLogData parameterData)
        {
            var builder = new StringBuilder();

            var value = parameterData.Value;
            var clrType = value?.GetType();

            FormatParameterValue(builder, value);

            if (parameterData.IsNullable
                && value != null
                && !IsNullableType(clrType))
            {
                builder.Append(" (Nullable = true)");
            }
            else
            {
                if (!parameterData.IsNullable
                    && parameterData.HasValue
                    && (value == null
                        || IsNullableType(clrType)))
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

        private static void FormatParameterValue(StringBuilder builder, object parameterValue)
        {
            if (parameterValue.GetType() != typeof(byte[]))
            {
                builder.Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture));
                return;
            }

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

        private static bool IsNullableType(Type type)
        {
            var typeInfo = type.GetTypeInfo();

            return !typeInfo.IsValueType
                   || (typeInfo.IsGenericType
                       && (typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        private static bool IsNormalDbType(DbType dbType, Type clrType)
        {
            if (clrType == null)
            {
                return false;
            }

            clrType = UnwrapEnumType(UnwrapNullableType(clrType));

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

        private static Type UnwrapNullableType(Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        private static Type UnwrapEnumType(Type type)
            => !type.GetTypeInfo().IsEnum ? type : Enum.GetUnderlyingType(type);
    }
}
