// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
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
            _options = BuildOptions();

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }
        }

        public override DbContextOptions BuildOptions(IServiceCollection serviceCollection = null)
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase()
                .UseInternalServiceProvider(
                    (serviceCollection ?? new ServiceCollection())
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                        .AddSingleton<ILoggerFactory>(_testLoggerFactory)
                        .BuildServiceProvider()).Options;

        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            => new NorthwindContext(_options, queryTrackingBehavior);
    }

    public class TestLoggerFactory : ILoggerFactory
    {
        public static ITestOutputHelper TestOutputHelper;

        private readonly TestLogger _logger = new TestLogger();

        private class TestLogger : ILogger
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => TestOutputHelper?.WriteLine(formatter(state, exception));

            public bool IsEnabled(LogLevel logLevel) => TestOutputHelper != null;

            public IDisposable BeginScope<TState>(TState state) => new NullDisposable();

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
