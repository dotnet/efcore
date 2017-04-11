// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class InterceptingLoggerTest
    {
        [Fact]
        public void Can_filter_for_messages_of_one_category()
        {
            FilterTest(c => c == LoggerCategory.Database.Sql.Name, "SQL1", "SQL2");
        }

        [Fact]
        public void Can_filter_for_messages_of_one_subcategory()
        {
            FilterTest(c => c.StartsWith(LoggerCategory.Database.Name), "DB1", "SQL1", "DB2", "SQL2");
        }

        [Fact]
        public void Can_filter_for_all_EF_messages()
        {
            FilterTest(c => c.StartsWith(LoggerCategory.Root), "DB1", "SQL1", "Query1", "DB2", "SQL2", "Query2");
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

            var dbLogger = new InterceptingLogger<LoggerCategory.Database>(loggerFactory, new LoggingOptions());
            var sqlLogger = new InterceptingLogger<LoggerCategory.Database.Sql>(loggerFactory, new LoggingOptions());
            var queryLogger = new InterceptingLogger<LoggerCategory.Query>(loggerFactory, new LoggingOptions());
            var randomLogger = loggerFactory.CreateLogger("Random");

            dbLogger.LogInformation("DB1");
            sqlLogger.LogInformation("SQL1");
            queryLogger.LogInformation("Query1");
            randomLogger.LogInformation("Random1");

            dbLogger.LogInformation("DB2");
            sqlLogger.LogInformation("SQL2");
            queryLogger.LogInformation("Query2");
            randomLogger.LogInformation("Random2");

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

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                    Func<TState, Exception, string> formatter)
                    => _messages?.Add(formatter(state, exception));

                public IDisposable BeginScope<TState>(TState state) => null;
            }
        }
    }
}
