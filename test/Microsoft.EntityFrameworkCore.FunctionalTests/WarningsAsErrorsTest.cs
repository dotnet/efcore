// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local

// ReSharper disable ClassNeverInstantiated.Local
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class WarningsAsErrorsTest
    {
        [Fact]
        public void Throws_when_warning_as_error()
        {
            using (var context = new WarningAsErrorContext())
            {
                Assert.Equal(
                    CoreStrings.WarningAsError(
                        $"{nameof(CoreLoggingEventId)}.{nameof(CoreLoggingEventId.IncludeIgnoredWarning)}",
                        CoreStrings.LogIgnoredInclude("e.Nav")),
                    Assert.Throws<InvalidOperationException>(()
                        => context.WarningAsErrorEntities.Include(e => e.Nav).OrderBy(e => e.Id).Select(e => e.Id).ToList()).Message);
            }
        }

        private class WarningAsErrorContext : DbContext
        {
            public DbSet<WarningAsErrorEntity> WarningAsErrorEntities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseLoggerFactory(new FakeLoggerFactory())
                    .SetWarningsAsErrors()
                    .UseInMemoryDatabase();
        }

        private class WarningAsErrorEntity
        {
            public IncludedEntity Nav { get; set; }

            public string Id { get; set; }
        }

        private class IncludedEntity
        {
            public int Id { get; set; }
        }

        private class FakeLoggerFactory : ILoggerFactory
        {
            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName) => new FakeLogger();

            private class FakeLogger : ILogger
            {
                public void Log<TState>(
                    LogLevel logLevel,
                    EventId eventId,
                    TState state,
                    Exception exception,
                    Func<TState, Exception, string> formatter)
                {
                }

                public bool IsEnabled(LogLevel logLevel) => false;

                public IDisposable BeginScope<TState>(TState state) => null;
            }

            public void AddProvider(ILoggerProvider provider)
            {
            }
        }
    }
}
