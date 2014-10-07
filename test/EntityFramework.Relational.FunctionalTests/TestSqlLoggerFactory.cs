// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Framework.Logging;
#if ASPNETCORE50
using System.Threading;
#else
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
            return Logger ?? Init();
        }

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public SqlLogger Init()
        {
            var logger = new SqlLogger();
#if ASPNETCORE50
            _logger.Value = logger;
#else
            CallContext.LogicalSetData(ContextName, logger);
#endif
            return logger;
        }

        public static SqlLogger Logger
        {
            get
            {
#if ASPNETCORE50
                return _logger.Value;
#else
                return (SqlLogger)CallContext.LogicalGetData(ContextName);
#endif
            }
        }

        public class SqlLogger : ILogger
        {
            public readonly List<string> SqlStatements = new List<string>();

            private CancellationTokenSource _cancellationTokenSource;

            public void Write(
                TraceType eventType,
                int eventId,
                object state,
                Exception exception,
                Func<object, Exception, string> formatter)
            {
                if (eventId == RelationalLoggingEventIds.Sql)
                {
                    if (_cancellationTokenSource != null)
                    {
                        _cancellationTokenSource.Cancel();
                        _cancellationTokenSource = null;
                    }

                    var sql = formatter(state, exception);

                    SqlStatements.Add(sql);
                }
            }

            public bool IsEnabled(TraceType eventType)
            {
                return true;
            }

            public CancellationToken CancelQuery()
            {
                return (_cancellationTokenSource = new CancellationTokenSource()).Token;
            }

            public string Sql
            {
                get { return string.Join("\r\n\r\n", SqlStatements); }
            }

            public IDisposable BeginScope(object state)
            {
                return NullScope.Instance;
            }
        }

        public class NullScope : IDisposable
        {
            public static NullScope Instance = new NullScope();

            public void Dispose()
            {
                // intentionally does nothing
            }
        }
    }
}
