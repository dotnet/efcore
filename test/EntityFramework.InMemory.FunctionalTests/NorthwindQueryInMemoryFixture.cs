// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class NorthwindQueryInMemoryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        private readonly TestLoggerFactory _testLoggerFactory = new TestLoggerFactory();

        public NorthwindQueryInMemoryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryDatabase()
                    .ServiceCollection()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(_testLoggerFactory)
                    .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();
            _options = optionsBuilder.Options;

            using (var context = CreateContext())
            {
                NorthwindData.Seed(context);
            }
        }

        public override NorthwindContext CreateContext()
            => new NorthwindContext(_serviceProvider, _options);
    }

    public class TestLoggerFactory : ILoggerFactory
    {
        public static ITestOutputHelper TestOutputHelper;

        private readonly TestLogger _logger = new TestLogger();

        private class TestLogger : ILogger
        {
            public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                if (eventId == 6)
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

        public LogLevel MinimumLevel { get; set; }
    }
}
