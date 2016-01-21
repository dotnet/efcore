// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
#if !DNXCORE50
using System.Runtime.Remoting.Messaging;

#endif

namespace Microsoft.EntityFrameworkCore.FunctionalTests
{
    public class TestSqlLoggerFactory : ILoggerFactory
    {
        private static SqlLogger _logger;
        private static string EOL = Environment.NewLine;

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

        public static string Log => Logger.SqlLoggerData._log.ToString();

        public static string Sql
            => string.Join(EOL + EOL, Logger.SqlLoggerData._sqlStatements);

        public static List<string> SqlStatements => Logger.SqlLoggerData._sqlStatements;

        private class SqlLoggerData
        {
            // ReSharper disable InconsistentNaming
            public readonly IndentedStringBuilder _log = new IndentedStringBuilder();
            public readonly List<string> _sqlStatements = new List<string>();
            public ITestOutputHelper _testOutputHelper;
            public CancellationTokenSource _cancellationTokenSource;
            // ReSharper restore InconsistentNaming
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class SqlLogger : ILogger
        {
#if DNXCORE50
            private readonly static AsyncLocal<SqlLoggerData> _loggerData = new AsyncLocal<SqlLoggerData>();
#else
            private const string ContextName = "__SQL";
#endif

            // ReSharper disable once MemberCanBeMadeStatic.Local
            public SqlLoggerData SqlLoggerData
            {
                get
                {
#if DNXCORE50
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
#if DNXCORE50
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
                                        .Select(kv => kv.Key + ": "
                                                      + Convert.ToString(kv.Value, CultureInfo.InvariantCulture)))
                                  + EOL + EOL;
                        }

                        sqlLoggerData._sqlStatements.Add(parameters + commandLogData.CommandText);
                    }

                    else
                    {
                        sqlLoggerData._log.AppendLine(format);
                    }

                    sqlLoggerData._testOutputHelper?.WriteLine(format + Environment.NewLine);
                }
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScopeImpl(object state) => SqlLoggerData._log.Indent();

            // ReSharper disable once MemberCanBeMadeStatic.Local
            public void ResetLoggerData()
                =>
#if DNXCORE50
                    _loggerData.Value = null;
#else
                    CallContext.LogicalSetData(ContextName, null);
#endif
        }
    }
}
