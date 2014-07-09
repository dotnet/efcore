// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;
#if K10
using System.Threading;
#else
using System.Runtime.Remoting.Messaging;
#endif

namespace Microsoft.Data.Entity
{
    public class TestSqlLoggerFactory : ILoggerFactory
    {
#if K10
        private readonly static AsyncLocal<SqlLogger> _logger = new AsyncLocal<SqlLogger>();
#else
        private const string ContextName = "__SQL";
#endif

        public ILogger Create(string name)
        {
            return Logger ?? Init();
        }

        public SqlLogger Init()
        {
            var logger = new SqlLogger();
#if K10
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
#if K10
                return _logger.Value;
#else
                return (SqlLogger)CallContext.LogicalGetData(ContextName);
#endif
            }
        }

        public class SqlLogger : ILogger
        {
            public readonly List<string> _sqlStatements = new List<string>();

            public bool WriteCore(
                TraceType eventType,
                int eventId,
                object state,
                Exception exception,
                Func<object, Exception, string> formatter)
            {
                if (eventId == RelationalLoggingEventIds.Sql)
                {
                    var sql = formatter(state, exception);

                    _sqlStatements.Add(sql);

                    //Trace.WriteLine(sql);
                }

                return true;
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
