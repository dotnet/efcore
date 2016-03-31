// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class NorthwindQueryInMemoryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        private readonly TestLoggerFactory _testLoggerFactory = new TestLoggerFactory();

        public NorthwindQueryInMemoryFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(_testLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider).Options;

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }
        }

        public override NorthwindContext CreateContext()
            => new NorthwindContext(_options);
    }

    public class TestLoggerFactory : ILoggerFactory
    {
        public static ITestOutputHelper TestOutputHelper;

        private readonly TestLogger _logger = new TestLogger();

        private class TestLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (eventId.Id == 6)
                {
                    TestOutputHelper.WriteLine(formatter(state, exception));
                }
            }

            public bool IsEnabled(LogLevel logLevel) => TestOutputHelper != null;

            public IDisposable BeginScopeImpl(object state) => new NullDisposable();

            private class NullDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
