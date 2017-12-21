// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryTransactionManagerTest
    {
        [Fact]
        public void CurrentTransaction_returns_null()
        {
            var transactionManager = new InMemoryTransactionManager(CreateLogger());

            Assert.Null(transactionManager.CurrentTransaction);
        }

        [Fact]
        public void Throws_on_BeginTransaction()
        {
            AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).BeginTransaction());
        }

        [Fact]
        public void Throws_on_BeginTransactionAsync()
        {
            AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).BeginTransactionAsync().GetAwaiter().GetResult());
        }

        [Fact]
        public void Throws_on_CommitTransaction()
        {
            AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).CommitTransaction());
        }

        [Fact]
        public void Throws_on_RollbackTransaction()
        {
            AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).RollbackTransaction());
        }

        private static void AssertThrows(Action action)
        {
            Assert.Equal(
                CoreStrings.WarningAsErrorTemplate(
                    InMemoryEventId.TransactionIgnoredWarning,
                    InMemoryStrings.LogTransactionsNotSupported.GenerateMessage()),
                Assert.Throws<InvalidOperationException>(action).Message);
        }

        public List<(LogLevel Level, EventId Id, string Message)> Log { get; }
            = new List<(LogLevel, EventId, string)>();

        private DiagnosticsLogger<DbLoggerCategory.Database.Transaction> CreateLogger()
        {
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().ConfigureWarnings(w => w.Default(WarningBehavior.Throw)).Options);
            var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                new ListLoggerFactory(Log, l => l == DbLoggerCategory.Database.Transaction.Name),
                options,
                new DiagnosticListener("Fake"));
            return logger;
        }
    }
}
