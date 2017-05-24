// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryTransactionManagerTest
    {
        [Fact]
        public void CurrentTransaction_returns_null()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            var transactionManager = new InMemoryTransactionManager(new FakeLogger());

            Assert.Null(transactionManager.CurrentTransaction);
        }

        [Fact]
        public void Throws_on_BeginTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            var transactionManager = new InMemoryTransactionManager(new FakeLogger());

            Assert.Equal(
                InMemoryStrings.LogTransactionsNotSupported.GenerateMessage(),
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.BeginTransaction()).Message);
        }

        [Fact]
        public async Task Throws_on_BeginTransactionAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            var transactionManager = new InMemoryTransactionManager(new FakeLogger());

            Assert.Equal(
                InMemoryStrings.LogTransactionsNotSupported.GenerateMessage(),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await transactionManager.BeginTransactionAsync())).Message);
        }

        [Fact]
        public void Throws_on_CommitTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            var transactionManager = new InMemoryTransactionManager(new FakeLogger());

            Assert.Equal(
                InMemoryStrings.LogTransactionsNotSupported.GenerateMessage(),
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.CommitTransaction()).Message);
        }

        [Fact]
        public void Throws_on_RollbackTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

            var transactionManager = new InMemoryTransactionManager(new FakeLogger());

            Assert.Equal(
                InMemoryStrings.LogTransactionsNotSupported.GenerateMessage(),
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.RollbackTransaction()).Message);
        }

        private class FakeLogger : IDiagnosticsLogger<DbLoggerCategory.Database.Transaction>, ILogger
        {
            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                throw new InvalidOperationException(formatter(state, exception));
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public WarningBehavior GetLogBehavior(EventId eventId, LogLevel logLevel) => WarningBehavior.Log;

            public IDisposable BeginScope<TState>(TState state) => null;

            public ILoggingOptions Options { get; }

            public bool ShouldLogSensitiveData() => false;

            public ILogger Logger => this;

            public DiagnosticSource DiagnosticSource { get; } = new DiagnosticListener("Fake");
        }
    }
}
