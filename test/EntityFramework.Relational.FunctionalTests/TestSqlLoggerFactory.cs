// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Xunit.Abstractions;
#if !DNXCORE50
using System.Runtime.Remoting.Messaging;

#endif

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class TestSqlLoggerFactory : ILoggerFactory
    {
#if DNXCORE50
        private readonly static AsyncLocal<SqlLogger> _logger = new AsyncLocal<SqlLogger>();
#else
        private const string ContextName = "__SQL";
#endif

        public LogLevel MinimumLevel { get; set; }

        public ILogger CreateLogger(string name)
        {
            return Logger;
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        private static SqlLogger Init()
        {
            var logger = new SqlLogger();
#if DNXCORE50
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
#if DNXCORE50
                var logger = _logger.Value;
#else
                var logger = (SqlLogger)CallContext.LogicalGetData(ContextName);
#endif
                return logger ?? Init();
            }
        }

        public static CancellationToken CancelQuery()
        {
            Logger._cancellationTokenSource = new CancellationTokenSource();

            return Logger._cancellationTokenSource.Token;
        }

        public static void Reset()
        {
#if DNXCORE50
            _logger.Value = null;
#else
            CallContext.LogicalSetData(ContextName, null);
#endif
        }

        public static void CaptureOutput(ITestOutputHelper testOutputHelper)
        {
            Logger._testOutputHelper = testOutputHelper;
        }

        public static string Log => Logger._log.ToString();
        public static string Sql => string.Join(Environment.NewLine + Environment.NewLine, Logger._sqlStatements);
        public static List<string> SqlStatements => Logger._sqlStatements;

        private class SqlLogger : ILogger
        {
            // ReSharper disable InconsistentNaming
            public readonly IndentedStringBuilder _log = new IndentedStringBuilder();
            public readonly List<string> _sqlStatements = new List<string>();

            public ITestOutputHelper _testOutputHelper;
            public CancellationTokenSource _cancellationTokenSource;
            // ReSharper restore InconsistentNaming

            public void Log(
                LogLevel logLevel,
                int eventId,
                object state,
                Exception exception,
                Func<object, Exception, string> formatter)
            {
                var format = formatter(state, exception)?.Trim();

                if (format != null)
                {
                    if (eventId == RelationalLoggingEventIds.ExecutingSql)
                    {
                        if (_cancellationTokenSource != null)
                        {
                            _cancellationTokenSource.Cancel();
                            _cancellationTokenSource = null;
                        }

                        _sqlStatements.Add(format);
                    }
                    else
                    {
                        _log.AppendLine(format);
                    }

                    _testOutputHelper?.WriteLine(format + Environment.NewLine);
                }
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScopeImpl(object state)
            {
                return _log.Indent();
            }
        }
    }
}
