// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Microsoft.Data.Entity.Relational;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Tests
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
            return Logger;
        }

        public void Init()
        {
#if K10
            _logger.Value = new SqlLogger();
#else
            CallContext.LogicalSetData(ContextName, new SqlLogger());
#endif
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
        }
    }
}
