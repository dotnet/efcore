// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class DiagnosticsLoggerTest
    {
        [Fact]
        public void Can_filter_for_messages_of_one_category()
        {
            FilterTest(c => c == DbLoggerCategory.Database.Command.Name, "SQL1", "SQL2");
        }

        [Fact]
        public void Can_filter_for_messages_of_one_subcategory()
        {
            FilterTest(c => c.StartsWith(DbLoggerCategory.Database.Name), "DB1", "SQL1", "DB2", "SQL2");
        }

        [Fact]
        public void Can_filter_for_all_EF_messages()
        {
            FilterTest(c => c.StartsWith(DbLoggerCategory.Name), "DB1", "SQL1", "Query1", "DB2", "SQL2", "Query2");
        }

        [Fact]
        public void Can_get_all_messages()
        {
            FilterTest(c => true, "DB1", "SQL1", "Query1", "Random1", "DB2", "SQL2", "Query2", "Random2");
        }

        private void FilterTest(Func<string, bool> filter, params string[] expected)
        {
            var loggerFactory = new LoggerFactory();
            var loggerProvider = new TestLoggerProvider(filter);

            loggerFactory.AddProvider(loggerProvider);

            var dbLogger = new DiagnosticsLogger<DbLoggerCategory.Database>(loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"));
            var sqlLogger = new DiagnosticsLogger<DbLoggerCategory.Database.Command>(loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"));
            var queryLogger = new DiagnosticsLogger<DbLoggerCategory.Query>(loggerFactory, new LoggingOptions(), new DiagnosticListener("Fake"));
            var randomLogger = loggerFactory.CreateLogger("Random");

            dbLogger.Logger.LogInformation(1, "DB1");
            sqlLogger.Logger.LogInformation(2, "SQL1");
            queryLogger.Logger.LogInformation(3, "Query1");
            randomLogger.LogInformation(4, "Random1");

            dbLogger.Logger.LogInformation(1, "DB2");
            sqlLogger.Logger.LogInformation(2, "SQL2");
            queryLogger.Logger.LogInformation(3, "Query2");
            randomLogger.LogInformation(4, "Random2");

            Assert.Equal(loggerProvider.Messages, expected);
        }

        public class TestLoggerProvider : ILoggerProvider
        {
            private readonly Func<string, bool> _filter;

            public TestLoggerProvider(Func<string, bool> filter)
            {
                _filter = filter;
            }

            public List<string> Messages { get; } = new List<string>();

            public ILogger CreateLogger(string categoryName)
                => _filter(categoryName) ? new TestLogger(Messages) : new TestLogger(null);

            public void Dispose()
            {
            }

            private class TestLogger : ILogger
            {
                private readonly List<string> _messages;

                public TestLogger(List<string> messages)
                {
                    _messages = messages;
                }

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(
                    LogLevel logLevel, EventId eventId, TState state, Exception exception,
                    Func<TState, Exception, string> formatter)
                    => _messages?.Add(formatter(state, exception));

                public IDisposable BeginScope<TState>(TState state) => null;
            }
        }
    }
}
