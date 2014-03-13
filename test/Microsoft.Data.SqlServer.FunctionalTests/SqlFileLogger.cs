// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Relational;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    // Watch the log in PS with: "tail -f $env:userprofile\.klog\sql.log"
    public class SqlFileLogger : ILogger
    {
        public static readonly ILogger Instance = new SqlFileLogger();

        private readonly string _logFilePath;

        private SqlFileLogger()
        {
            var logDirectory
                = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), ".klog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, "sql.log");
        }

        public bool WriteCore(
            TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (formatter != null
                && eventId == RelationalLoggingEventIds.Sql)
            {
                var message = formatter(state, exception);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    lock (_logFilePath)
                    {
                        File.AppendAllText(_logFilePath, message + "\r\n");
                    }
                }
            }

            return true;
        }
    }
}
