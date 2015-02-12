// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
#if !ASPNETCORE50
using System.Runtime.Remoting.Messaging;

#endif

namespace Microsoft.Data.Entity.Relational.FunctionalTests
{
    public class TestSqlLoggerFactory : ILoggerFactory
    {
#if ASPNETCORE50
        private readonly static AsyncLocal<SqlLogger> _logger = new AsyncLocal<SqlLogger>();
#else
        private const string ContextName = "__SQL";
#endif

        public ILogger Create(string name)
        {
            return Logger;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        private static SqlLogger Init()
        {
            var logger = new SqlLogger();
#if ASPNETCORE50
            _logger.Value = logger;
#else
            CallContext.LogicalSetData(ContextName, logger);
#endif
            return logger;
        }

        private static SqlLogger Logger
        {
            get
            {
#if ASPNETCORE50
                var logger = _logger.Value;
#else
                var logger = (SqlLogger)CallContext.LogicalGetData(ContextName);
#endif
                return logger ?? Init();
            }
        }

        public static CancellationToken CancelQuery()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Logger.CancellationTokenSource = cancellationTokenSource;
            return cancellationTokenSource.Token;
        }

        public static string Log
        {
            get { return Logger.Log; }
        }

        public static string Sql
        {
            get { return Logger.Sql; }
        }

        public static List<string> SqlStatements
        {
            get { return Logger.SqlStatements; }
        }

        private class SqlLogger : ILogger
        {
            private readonly IndentedStringBuilder _log = new IndentedStringBuilder();

            private CancellationTokenSource _cancellationTokenSource;

            public void Write(
                LogLevel logLevel,
                int eventId,
                object state,
                Exception exception,
                Func<object, Exception, string> formatter)
            {
                var format = formatter(state, exception);

                if (eventId == RelationalLoggingEventIds.Sql)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource = null;
                    }

                    SqlStatements.Add(format);
                }
                else
                {
                    _log.AppendLine(format);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public CancellationTokenSource CancellationTokenSource
            {
                set { _cancellationTokenSource = value; }
            }

            public List<string> SqlStatements { get; } = new List<string>();

            public string Sql
            {
                get { return string.Join("\r\n\r\n", SqlStatements); }
            }

            public string Log
            {
                get { return string.Join("\r\n", _log); }
            }

            public IDisposable BeginScope(object state)
            {
                return _log.Indent();
            }
        }
    }
}
