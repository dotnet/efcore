// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.FunctionalTests
{
    public class SqlServerDatabaseModelFixture : IDisposable
    {
        public SqlServerDatabaseModelFixture()
        {
            TestStore = SqlServerTestStore.Create("SqlServerDatabaseModelTest");
        }

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null, ILogger logger = null)
        {
            TestStore.ExecuteNonQuery(createSql);

            return new SqlServerDatabaseModelFactory(new TestLoggerFactory(logger).CreateLogger<SqlServerDatabaseModelFactory>())
                .Create(TestStore.ConnectionString, selection ?? TableSelectionSet.All);
        }

        public SqlServerTestStore TestStore { get; }

        public void ExecuteNonQuery(string sql) => TestStore.ExecuteNonQuery(sql);

        public void Dispose() => TestStore.Dispose();

        private class TestLoggerFactory : ILoggerFactory
        {
            private readonly ILogger _logger;

            public TestLoggerFactory(ILogger logger)
            {
                _logger = logger ?? new TestLogger();
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose()
            {
            }
        }

        private class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();

            public void Dispose()
            {
            }
        }

        public class TestLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => Items.Add(new { logLevel, eventId, state, exception });

            public bool IsEnabled(LogLevel logLevel) => true;

            public ICollection<dynamic> Items = new List<dynamic>();
        }
    }
}
