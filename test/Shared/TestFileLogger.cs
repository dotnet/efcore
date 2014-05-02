// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.IO;
using Microsoft.AspNet.Logging;

namespace Microsoft.Data.Entity.Tests
{
    // Watch the log in PS with: "tail -f $env:userprofile\.klog\data-test.log"
    public class TestFileLogger : ILogger
    {
        public static readonly ILoggerFactory Factory = new TestFileLoggerFactory();

        private class TestFileLoggerFactory : ILoggerFactory
        {
            public ILogger Create(string name)
            {
                return Instance;
            }
        }

        public static readonly ILogger Instance = new TestFileLogger();

        private readonly string _logFilePath;

        protected TestFileLogger(string fileName = "data-test.log")
        {
            var logDirectory
                = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), ".klog");

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, fileName);
        }

        public bool WriteCore(
            TraceType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (formatter != null)
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
