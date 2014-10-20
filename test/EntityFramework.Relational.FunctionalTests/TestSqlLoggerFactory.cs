// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Data.Entity.Utilities;
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
            private readonly List<string> _sqlStatements = new List<string>();

            private readonly IndentedStringBuilder _log = new IndentedStringBuilder();

            private CancellationTokenSource _cancellationTokenSource;

            public void Write(
                TraceType eventType,
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

                    _sqlStatements.Add(format);
                }
                else
                {
                    _log.AppendLine(format);
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

            public List<string> SqlStatements
            {
                get { return _sqlStatements; }
            }

            public string Sql
            {
                get { return string.Join("\r\n\r\n", _sqlStatements); }
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
