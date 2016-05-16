// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable ClassNeverInstantiated.Local
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class WarningsAsErrorsTest
    {
        [Fact]
        public void Throws_when_warning_as_error_all()
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

        [Fact]
        public void Throws_when_warning_as_error_specific()
        {
            using (var context = new WarningAsErrorContext(CoreLoggingEventId.IncludeIgnoredWarning))
            {
                Assert.Equal(
                    CoreStrings.WarningAsError(
                        $"{nameof(CoreLoggingEventId)}.{nameof(CoreLoggingEventId.IncludeIgnoredWarning)}",
                        CoreStrings.LogIgnoredInclude("e.Nav")),
                    Assert.Throws<InvalidOperationException>(()
                        => context.WarningAsErrorEntities.Include(e => e.Nav).Skip(1).Select(e => e.Id).ToList()).Message);
            }
        }

        [Fact]
        public void No_throw_when_event_id_not_registered()
        {
            using (var context = new WarningAsErrorContext(CoreLoggingEventId.SensitiveDataLoggingEnabledWarning))
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                context.WarningAsErrorEntities.Include(e => e.Nav).Take(1).Select(e => e.Id).ToList();
            }
        }

        private class WarningAsErrorContext : DbContext
        {
            private readonly CoreLoggingEventId[] _eventIds;

            public WarningAsErrorContext(params CoreLoggingEventId[] eventIds)
            {
                _eventIds = eventIds;
            }

            public DbSet<WarningAsErrorEntity> WarningAsErrorEntities { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseLoggerFactory(new FakeLoggerFactory())
                    .UseInMemoryDatabase()
                    .SetWarningsAsErrors(_eventIds);
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
